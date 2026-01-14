using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Project
{
    public class ProjectClassroomService : IProjectClassroomService
    {
        private readonly LmsDbContext _context;
        
        public ProjectClassroomService(LmsDbContext context)
        {
            _context = context;
        }

        //==========SERVICE TO CREATE CLASSROOM FOR THAT PROJECT==========
        public async Task<string> CreateProjectClassroomAsync(CreateProjectClassroomDto dto, string staffId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ClassroomName))
                throw new ArgumentException("ClassroomName không được để trống", nameof(dto.ClassroomName));

            if (string.IsNullOrWhiteSpace(staffId))
                throw new ArgumentException("CreatorLecturerId không được để trống", nameof(dto.CreatorLecturerID));

            // Sử dụng transaction để đảm bảo atomic
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var projectEntity = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == dto.ProjectID);

            try
            {
                //1. Create ID and InviteCode
                var proClassId = Uuidv7Generator.NewUuid7().ToString();
                var inviteCode = InviteCodeGenerator.GenerateInviteCode(8);

                //Create recode for ProjectClassroom
                var classroom = new ProjectClassroom
                {
                    ProClassId = proClassId,
                    ProjectId = projectEntity.ProjectId,
                    ClassroomName = dto.ClassroomName,
                    InviteCode = inviteCode,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.ProjectClassrooms.AddAsync(classroom);

                var member = new ProjectClassMem
                {
                    ProClassMemId = Uuidv7Generator.NewUuid7().ToString(),
                    ProClassId = proClassId,
                    LecturerId = staffId,
                    RoleInClass = "Lecturer"
                };

                await _context.ProjectClassMems.AddAsync(member);

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = dto.CreatorLecturerID,
                    DepartId = _context.StaffDeparts
                            .Where(s => s.StaffId == staffId)
                            .Select(s => s.DepartId)
                            .FirstOrDefault(),
                    Action = "Create Project Classroom",
                    TargetTable = "ProjectClassroom",
                    TargetId = proClassId,
                    TargetName = dto.ClassroomName,
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);

                //Save change
                await _context.SaveChangesAsync();

                //Commit transaction
                await transaction.CommitAsync();

                return proClassId;
            }
            catch
            {
                //Rollback automatic when using 'await uing'
                throw;
            }
        }

        //==========SERVICE TO TAKE ALL PROJECT CLASSROOM==========
        public async Task<List<MyClassroomDto>> GetMyClassroomsAsync(string staffId)
        {
            var query =
                from cm in _context.ProjectClassMems
                join cl in _context.ProjectClassrooms
                    on cm.ProClassId equals cl.ProClassId
                join pj in _context.Projects
                    on cl.ProjectId equals pj.ProjectId

                //Find lecturer for each classroom
                join lec in
                    (
                        from m in _context.ProjectClassMems
                        join s in _context.DepartmentStaffs on m.LecturerId equals s.StaffId
                        where m.RoleInClass == "Lecturer"
                        select new
                        {
                            m.ProClassId,
                            LecturerName = s.LastName
                        }
                    )
                on cl.ProClassId equals lec.ProClassId

                //Filler classroom this staff joining
                where cm.LecturerId == staffId

                select new MyClassroomDto
                {
                    ProClassID = cl.ProClassId,
                    ClassroomName = cl.ClassroomName,
                    ProjectTitle = pj.Title,
                    LecturerName = lec.LecturerName
                };

            return await query.Distinct().ToListAsync();
        }

        //==========SERVICE TO ADD LECTURER INTO CLASSROOM==========
        //-----ADD NEW LECTURER TO CLASSROOM
        //-----ONLY MEMBER ROLEINCLASS = LECTURER CAN ADD
        public async Task AddMemberAsync(AddLecturerToProjectClassroomDto dto, string staffId)
        {

            if (string.IsNullOrWhiteSpace(dto.ProClassID))
                throw new ArgumentException("ProClassId is null or error.", nameof(dto.ProClassID));

            if (string.IsNullOrWhiteSpace(dto.LecturerID))
                throw new ArgumentException("LecturerId is null or error.", nameof(dto.LecturerID));

            if (string.IsNullOrWhiteSpace(dto.NewMemberLecturerID))
                throw new ArgumentException("LecturerId is null or error.", nameof(dto.LecturerID));

            //Take classroom
            var classroom = await _context.ProjectClassrooms
                .FirstOrDefaultAsync(c => c.ProClassId == dto.ProClassID);
            if (classroom == null)
                throw new InvalidOperationException("Project Classroom is not exists or error!");

            //Check role of people who add
            var isLecturer = await _context.ProjectClassMems
                .AnyAsync(m => m.ProClassId == dto.ProClassID
                && m.LecturerId == staffId
                && m.RoleInClass == "Lecturer");

            if (!isLecturer)
                throw new UnauthorizedAccessException("Bạn không có quyền thêm thành viên vào lớp này.");

            //Check the user is exists
            var exists = await _context.ProjectClassMems
                .AnyAsync(m => m.ProClassId == dto.ProClassID
                            && m.LecturerId == dto.NewMemberLecturerID);

            if (exists)
                throw new InvalidOperationException("That member are in class now");

            var proClassMemID = Uuidv7Generator.NewUuid7().ToString();

            //Write DB recode
            var newMember = new ProjectClassMem
            {
                ProClassMemId = proClassMemID,
                ProClassId = dto.ProClassID,
                LecturerId = dto.NewMemberLecturerID,
                RoleInClass = "Lecturer"
            };

            await _context.ProjectClassMems.AddAsync(newMember);

            //Write Log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = dto.LecturerID,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == dto.LecturerID)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Add New Lecturers",
                TargetTable = "ProjectClassMem",
                TargetId = proClassMemID,
                TargetName = dto.NewMemberLecturerID,
                Timestap = DateTime.UtcNow
            };

            await _context.ActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();
        }

        //==========SERVICE TO GET ALL CONTENT OR MEETING NOTICE==========
        public async Task<ProjectClassroomDashboardDto> GetDashboardAsync(string proClassId)
        {
            if (string.IsNullOrWhiteSpace(proClassId))
                throw new ArgumentException("proClassId is required");

            //take infor classroom
            var classroom = await _context.ProjectClassrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ProClassId == proClassId);

            if (classroom == null)
                throw new InvalidOperationException("Project classroom not found");

            //Take content list 
            var contents = await _context.ProjectContents
                .Where(c => c.ProClassId == proClassId)
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new ProjectContentItemDto
                {
                    ProContentID = c.ProContentId,
                    Title = c.Title,
                    ContentType = c.ContentType,
                    CreatedDate = c.CreateDate,

                    //If it's Assignment -> have Deadline
                    Deadline = (c.ContentType == "AssignmentLate"
                                || c.ContentType == "AssignmentNoLate")
                                ? c.Deadline
                                : null,
                    //Take poster name
                    PostedByName = _context.DepartmentStaffs
                        .Where(s => s.StaffId == c.PostedBy)
                        .Select(s => s.LastName)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();

            //Take Weekly Meeting
            var meetings = await (
                from m in _context.WeeklyMeetings
                where m.ProClassId == proClassId
                select new ProjectContentItemDto
                {
                    ProContentID = m.MeetingId.ToString(),
                    Title = $"Weekly Meeting",
                    ContentType = "WeeklyMeeting",
                    Deadline = m.MeetingDate,
                    
                    //Take poster name
                    PostedByName = _context.DepartmentStaffs
                        .Where(s => s.StaffId == m.LecturerId)
                        .Select(s => s.LastName)
                        .FirstOrDefault() ?? "Unknown"
                }
             ).ToListAsync();

            return new ProjectClassroomDashboardDto
            {
                ProClassID = classroom.ProClassId,
                ClassroomName = classroom.ClassroomName,
                ProjectID = proClassId,
                InviteCode = classroom.InviteCode,
                CreateDate = classroom.CreateDate,

                Contents = contents
            };
        }

        //==========SERVICE TO CREATE CONTENT==========
        public async Task CreateProjectContentAsync(CreateProjectContentDto dto, string staffId)
        {
            var validTypes = new[] { "Notice", "AssignmentLate", "AssignmentNoLate" };

            if (!validTypes.Contains(dto.ContentType))
                throw new ArgumentException("Content Type doesn't valid!");

            bool isAssigment = dto.ContentType == "AssigmentLate" || dto.ContentType == "AssigmentNoLate";

            var menber = await _context.ProjectClassMems
                .FirstOrDefaultAsync(x =>
                    x.ProClassId == dto.ProClassID && x.LecturerId == staffId);

            if (menber == null)
                throw new UnauthorizedAccessException("You doesn't member in this class");

            //Take Classroom name so set folder name
            var classroom = await _context.ProjectClassrooms
                .FirstOrDefaultAsync(c => c.ProClassId == dto.ProClassID);


            //Create Content
            string contentId = Uuidv7Generator.NewUuid7().ToString();
            var newContent = new ProjectContent
            {
                ProContentId = contentId,
                ProClassId = dto.ProClassID,
                PostedBy = staffId,
                Title = dto.Title,
                ContentText = dto.ContentText,
                ContentType = dto.ContentType,
                Deadline = dto.Deadline,
                CreateDate = DateTime.UtcNow
            };

            await _context.ProjectContents.AddAsync(newContent);

            //Handle upload file if have
            if (dto.Files != null && dto.Files.Count > 0)
            {
                string uploadFolder = Path.Combine("uploads", "ProjectClassConFiles", classroom.ClassroomName);

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach(var file in dto.Files)
                {
                    if (file.Length > 0)
                    {
                        string fileId = Uuidv7Generator.NewUuid7().ToString();
                        string fileName = file.FileName;
                        string filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var fileRecord = new ProConFile
                        {
                            ProConFileId = fileId,
                            ProContentId = contentId,
                            FileName = fileName,
                            FilePath = filePath.Replace("\\", "/"),
                            FileType = file.ContentType
                        };

                        await _context.ProConFiles.AddAsync(fileRecord);
                    }
                }
            }

            //Write log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == staffId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Create New Content",
                TargetTable = "ProjectContents",
                TargetId = contentId,
                TargetName = dto.Title,
                Timestap = DateTime.UtcNow,
            };

            await _context.ActivityLogs.AddAsync(log);

            //Save all to DB

            await _context.SaveChangesAsync();
        }

        //==========TAKE DETAIL CONTENT==========
        public async Task<ProjectContentDetailDto?> GetContentDetailAsync(string proClassId, string contentId)
        {


            //=====DATAIL CONTENT=====
            var content = await _context.ProjectContents
                .Include(c => c.ProConFiles)
                .FirstOrDefaultAsync(c => c.ProContentId == contentId
                           && c.ProClassId == proClassId);

            if (content == null)
                return null;

            //====TAKE PERSOL WHO POST=====
            var postedByName = await _context.DepartmentStaffs
                .Where(s => s.StaffId == content.PostedBy)
                .Select(s => s.LastName)
                .FirstOrDefaultAsync();

            //=====BUILD BASE DTO=====
            var dto = new ProjectContentDetailDto
            {
                ProContentID = content.ProContentId,
                Title = content.Title,
                ContentText = content.ContentText,
                PostedBy = postedByName,
                CreateDate = content.CreateDate,
                ContentType = content.ContentType,
                Deadline = content.Deadline,
                Files = content.ProConFiles.Select(f => new Application.DTOs.FileDto
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType
                }).ToList(),
            };

            //=====IF ASSINMENT ======
            if(content.ContentType == "AssignmentLate" || content.ContentType == "AssignmentNoLate")
            {
                //All student in classroom
                var studentIdsInClass = await _context.ProjectClassMems
                    .Where(m => m.ProClassId == content.ProClassId && m.AssignId != null)
                    .Select(m => m.AssignId)
                    .ToListAsync();

                //Submit
                var submissions = await _context.ProjectSubmissions
                    .Where(s => s.ProContentId == contentId)
                    .Include(s => s.ProjectSubmitFiles)
                    .ToListAsync();

                //List submit
                dto.ProStudentSubmitted = (from sub in  submissions
                                           join stu in _context.Students on sub.StudentId equals stu.StudentId
                                           select new ProjectStudentSubmitDto
                                           {
                                               StudentID = stu.StudentId,
                                               StudentName = stu.LastName + " " +stu.FirstName,
                                               SubmitDate = sub.SubmitDate,
                                               Score = sub.Score,
                                               Feedback = sub.FeedBack,
                                               Files = sub.ProjectSubmitFiles.Select(f => new Application.DTOs.FileDto
                                               {
                                                   FileName = f.FileName,
                                                   FilePath = f.FilePath,
                                                   FileType = f.FileType
                                               }).ToList()

                                           }).ToList();

                //List doesn't submit
                var submittedIDs = submissions.Select(s => s.StudentId).ToList();

                dto.ProStudentNotSubmitted = await _context.Students
                    .Where(s => studentIdsInClass.Contains(s.StudentId) && !submittedIDs.Contains(s.StudentId))
                    .Select(s => new PorjectStudentInfoDto
                    {
                        StudentId = s.StudentId,
                        StudentName = s.LastName + " " +s.FirstName
                    }).ToListAsync();
            }
            return dto;
        }

        //==========SERVICE TO UPDATE PROJECT CONTENT==========
        public async Task UpdateProjectContentAsync(ProContentUpdateDto dto, string staffId)
        {
            if (string.IsNullOrWhiteSpace(dto.ProContentID))
                throw new ArgumentException("ContentID doesn't exists");

            var content = await _context.ProjectContents
                .Include(x => x.ProConFiles)
                .FirstOrDefaultAsync(x => x.ProContentId == dto.ProContentID
                                        && x.ProClassId == dto.ProContentID);

            if (content == null)
                throw new Exception("Does not find the content");

            //Check update permission
            var member = await _context.ProjectClassMems
                .FirstOrDefaultAsync(x => x.ProClassId == dto.ProClassID
                                        && x.LecturerId == staffId);

            //Rule CHANGING CONTENT TYPE FROM NOTICE ↔ ASSIGNMENT IS NOT ALLOWED

            bool isOldAssignment = content.ContentType == "AssignmentLate"
                                || content.ContentType == "AssignmentNoLate";

            bool isNewAssignment = dto.ContentType == "AssignmentLate"
                                || dto.ContentType == "AssignmentNoLate";

            //conversion prohibited Notice -> Assignment and Assignment -> Notices
            if (isOldAssignment != isNewAssignment)
                throw new Exception("It is not possible to change the content type between Assignment and Notice.");

            if (!isOldAssignment)
            {
                dto.ContentType = "Notice";
                dto.Deadline = null;
            }

            //if assignment -> allow Late <-> NoLate transition
            if (isOldAssignment)
            {
                if (dto.ContentType != "AssignmentLate" && dto.ContentType != "AssignmentNoLate")
                    throw new Exception("Assignment type doesn't exists");

                content.Deadline = dto.Deadline;
            }

            //Update basic Infor Content
            content.Title = dto.Title;
            content.ContentText = dto.ContentText;
            content.ContentType = dto.ContentType;

            //Handle to delete file if have
            if(dto.FilesToDelete != null && dto.FilesToDelete.Any())
            {
                foreach(var fileId in dto.FilesToDelete)
                {
                    var file = await _context.ProConFiles.FindAsync(fileId);
                    if(file != null)
                    {
                        if (File.Exists(file.FilePath))
                            File.Delete(file.FilePath);

                        _context.ProConFiles.Remove(file);
                    }
                }
            }

            //Handle file if have new file
            if(dto.NewFiles != null && dto.NewFiles.Any())
            {
                var classroom = await _context.ProjectClassrooms
                    .FirstOrDefaultAsync(x => x.ProClassId == dto.ProClassID);

                string uploadFolder = Path.Combine("uploads", "ProjectClassConFiles", classroom.ClassroomName);

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.NewFiles)
                {
                    string fileId = Uuidv7Generator.NewUuid7().ToString();
                    string newFilePath = Path.Combine(uploadFolder, fileId + Path.GetExtension(file.FileName));

                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var newFile = new ProConFile
                    {
                        ProConFileId = fileId,
                        ProContentId = content.ProContentId,
                        FileName = file.FileName,
                        FilePath = newFilePath,
                        FileType = file.ContentType
                    };
                    _context.ProConFiles.Add(newFile);

                }
            }

            //Write Log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.DepartId == staffId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Create New Content",
                TargetTable = "ProjectContents",
                TargetId = dto.ProContentID,
                TargetName = dto.Title,
                Timestap = DateTime.UtcNow,
            };
            _context.ActivityLogs.Add(log);

        }

        //==========SERVICE TO DELETE PROJECT CONTENT==========
        public async Task DeleteProContentAsync(string proClassId, string contentId, string staffId)
        {
            if (string.IsNullOrWhiteSpace(proClassId) || string.IsNullOrWhiteSpace(contentId))
                throw new AggregateException("ProClassID or ContentID doesn't exists");
            //Check permissions

            var member = await _context.ProjectClassMems
                .FirstOrDefaultAsync(x => x.ProClassId == proClassId && x.LecturerId == staffId);

            if (member == null)
                throw new Exception("You not in this class");

            if (member.RoleInClass != "Lecturer" && member.RoleInClass != "Staff" && member.RoleInClass != "Admin")
                throw new Exception("You not have Permissions in this class");

            //Find Content
            var content = await _context.ProjectContents
                .Include(c => c.ProConFiles)
                .FirstOrDefaultAsync(c => c.ProContentId == contentId && c.ProContentId == proClassId);

            if (content == null)
                throw new Exception("doesn't find this content");

            //Delete all file content
            if (content.ProConFiles != null)
            {
                foreach(var f in content.ProConFiles)
                {
                    if (File.Exists(f.FilePath))
                        File.Delete(f.FilePath);

                    _context.ProConFiles.Remove(f);
                }
            }
            
            //Find all submit
            var submissions = await _context.ProjectSubmissions
                .Where(s => s.ProContentId == contentId)
                .ToListAsync();

            foreach (var sub in submissions)
            {
                //Delete all submission file
                var subFiles = await _context.ProjectSubmitFiles
                    .Where(sf => sf.ProSubmitId == sub.ProSubmitId)
                    .ToListAsync();

                foreach (var sf in subFiles)
                {
                    if(File.Exists(sf.FilePath))
                        File.Delete(sf.FilePath);

                    _context.ProjectSubmitFiles.Remove(sf);
                }

                //Delete Submissions
                _context.ProjectSubmissions.Remove(sub);
            }

            //Delete main content
            _context.ProjectContents.Remove(content);

            //Write log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == staffId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Delete Content" + content.Title,
                TargetTable = "ProjectContents",
                TargetId = contentId,
                TargetName = content.Title,
                Timestap = DateTime.UtcNow
            };

            await _context.ActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();
        }

    }
}
