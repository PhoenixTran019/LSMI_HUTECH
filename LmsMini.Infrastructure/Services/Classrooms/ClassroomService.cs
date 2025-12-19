using AutoMapper.Execution;
using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.Interfaces;
using LmsMini.Application.Models;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Classrooms
{
    public class ClassroomService : IClassroomService
    {
        private readonly LmsDbContext _context;
        

        public ClassroomService(LmsDbContext context)
        {
            _context = context;
        }

        //==========Service to create a new classroom==========
        public async Task<bool> CreateClassroomAsync (CreateClassroomDto dto, string staffId)
        {
            //Check if Classroom with the same name already exists
            if (await _context.Classrooms.AnyAsync(c => c.ClassName == dto.ClassName))
            {
                return false; // Classroom name already exists
            }

            var clasroomId = Uuidv7Generator.NewUuid7().ToString();
            var inviteCode = InviteCodeGenerator.GenerateInviteCode(8);

            var classSubEntity = await _context.Subjects.FirstOrDefaultAsync(d => d.SubId == dto.ClassSub);
            if (classSubEntity == null) throw new AggregateException("Invalid Subject");

            //If have main class, take Course of this

            // Khởi tạo mainClassEntity là null
            Class? mainClassEntity = null;

            // Chỉ kiểm tra MainClass nếu người dùng có cung cấp giá trị
            if (!string.IsNullOrWhiteSpace(dto.MainClass))
            {
                // Lấy thông tin MainClass từ DB
                mainClassEntity = await _context.Classes
                    .FirstOrDefaultAsync(cl => cl.ClassId == dto.MainClass);

                // Nếu người dùng cung cấp ID nhưng ID đó không tồn tại trong DB, thì báo lỗi.
                if (mainClassEntity == null)
                {
                    throw new AggregateException("Invalid Main Class ID provided");
                }
            }

            var classroom = new Classroom
            {
                ClassroomId = clasroomId,
                ClassName = dto.ClassName,
                ClassSub = classSubEntity.SubId,
                MainClass = mainClassEntity?.ClassId,
                Description = dto.Description,
                InviteCode = inviteCode,
                CreateBy = staffId,
                ClassStatus = dto.ClassStatus,
            };

            _context.Classrooms.Add(classroom);

            //Add the creator to the class as Teacher
            var createrMember = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = clasroomId,
                LecturerId = staffId,
                RoleInClass = "Teacher",
            };
            _context.ClassroomMembers.Add(createrMember);

            //If have main class, add the main class as a member too
            if (!string.IsNullOrEmpty(dto.MainClass))
            {
                var students = await _context.Students
                    .Where(s => s.ClassId == dto.MainClass)
                    .ToListAsync();

                foreach (var student in students)
                {
                    var member = new ClassroomMember
                    {
                        MemberId = Uuidv7Generator.NewUuid7().ToString(),
                        ClassroomId = clasroomId,
                        StudentId = student.StudentId,
                        RoleInClass = "Student",
                    };
                    _context.ClassroomMembers.Add(member);
                }
            }

            var staff = await _context.StaffDeparts
                .FirstOrDefaultAsync(s => s.StaffId == staffId);
            
            var departId = staff?.DepartId ?? "Unknown";

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = departId,
                Action = "Create Classroom",
                TargetTable = "Classrooms",
                TargetId = clasroomId,
                TargetName = dto.ClassName,
                Timestap = DateTime.UtcNow,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return true;
        }

        //==========Service to get all classrooms==========
        public async Task<List<ClassroomCardViewModel>> GetDashboardClassroomsAsync(ClassroomFilterDto filter, string role, string userId)
        {
            //Start the query from the Classrooms table and include related tables
            var query = _context.Classrooms
                .Include(c => c.ClassSubNavigation)
                .Include(c => c.MainClassNavigation)
                .Include(c => c.CreateByNavigation)
                .AsQueryable();

            //Search by filters (giữ nguyên)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(c => c.ClassName.Contains(filter.Keyword));

            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
                query = query.Where(c => c.ClassSub == filter.SubjectId);

            if (!string.IsNullOrWhiteSpace(filter.MainClassId))
                query = query.Where(c => c.MainClass == filter.MainClassId);

            if (!string.IsNullOrWhiteSpace(filter.Course))
                query = query.Where(c => c.MainClassNavigation.Course == filter.Course);

            // =========================================================
            // LỌC DỮ LIỆU DỰA TRÊN VAI TRÒ (ROLE)
            // =========================================================

            if (role != "Admin")
            {
                // Lấy StaffId/LecturerId từ UserId (vì Lecturer thường là Staff)
                var staffId = await _context.DepartmentStaffs
                    .Where(s => s.UserId == userId)
                    .Select(s => s.StaffId)
                    .FirstOrDefaultAsync();

                if (role == "Lecturer")
                {
                    // Nếu là Lecturer, chỉ được thấy các lớp họ là thành viên (LecturerId)
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Lấy danh sách ClassroomId mà Lecturer này là thành viên/người tạo
                        var memberClassroomIds = await _context.ClassroomMembers
                            .Where(m => m.LecturerId == userId)
                            .Select(m => m.ClassroomId)
                            .Distinct()
                            .ToListAsync();

                        // Áp dụng bộ lọc
                        query = query.Where(c => memberClassroomIds.Contains(c.ClassroomId));
                    }
                    else
                    {
                        // Nếu không tìm thấy StaffId/LecturerId, không có lớp nào được trả về.
                        query = query.Where(c => false);
                    }
                }
                else if (role == "Staff")
                {
                    // Nếu là Staff (quản lý phòng ban), dùng logic lọc cũ theo DepartId
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var allowedDepartIds = await _context.StaffDeparts
                            .Where(sd => sd.StaffId == userId)
                            .Select(sd => sd.DepartId)
                            .ToListAsync();

                        query = query.Where(c => allowedDepartIds.Contains(c.ClassSubNavigation.DepartId));
                    }
                    else
                    {
                        query = query.Where(c => false);
                    }
                }
            }
            // Admin sẽ bỏ qua bước lọc và xem tất cả.

            //Pagination and mapping to ViewModel (giữ nguyên)
            var result = await query
                .OrderByDescending(c => c.ClassroomId)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ClassroomCardViewModel
                {
                    ClassroomId = c.ClassroomId,
                    ClassName = c.ClassName,
                    ClassSub = c.ClassSubNavigation.SubName,
                    MainClassName = c.MainClassNavigation.ClassName,
                    Course = c.MainClassNavigation.Course,
                    LecturerName = c.CreateByNavigation.FirstName,
                    ClassStatus = c.ClassStatus,
                    InviteCode = c.InviteCode,
                })
                .ToListAsync();

            return result;
        }

        //==========SERVICE TO GET LIST MEMBER IN THIS CLASSROOM==========
        public async Task<IEnumerable<MemberInfoDto>> GetClassroomMembersAsync(string classroomId)
        {
            var classroomExists = await _context.Classrooms.AnyAsync(c => c.ClassroomId == classroomId);
            if (!classroomExists)
            {
                return Enumerable.Empty<MemberInfoDto>();
            }

            var lecturerMem = await _context.ClassroomMembers
                .Where(m => m.ClassroomId == classroomId && m.LecturerId != null)
                .Join(
                    _context.DepartmentStaffs,
                    member => member.LecturerId,
                    staff => staff.StaffId,
                    (member, staff) => new MemberInfoDto
                    {
                        MemberId = member.MemberId,
                        UserId = member.LecturerId,
                        FullName = staff.LastName + " " + staff.FirstName,
                        Email = staff.Mail,
                        RoleInClass = member.RoleInClass
                    }
                )
                .ToListAsync();

            var studentMem = await _context.ClassroomMembers
                .Where(m => m.ClassroomId == classroomId && m.StudentId != null)
                .Join(
                    _context.Students,
                    member => member.StudentId,
                    student => student.StudentId,
                    (member, student) => new MemberInfoDto
                    {
                        MemberId = member.MemberId,
                        UserId = member.StudentId,
                        FullName = student.LastName + " " + student.FirstName,
                        Email = student.Mail,
                        RoleInClass = member.RoleInClass
                    }
                )
                .ToListAsync();

            return lecturerMem
                .Concat(studentMem)
                .OrderBy(m => m.RoleInClass)
                .ToList();
        }

        //==========Service to add member to classroom==========
        public async Task<bool> AddMemberToClassroomAsync(string classroomId, string memberUserId, string role)
        {
            // 1. Kiểm tra Lớp học tồn tại
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            if (classroom == null)
            {
                throw new KeyNotFoundException($"Classroom with ID '{classroomId}' not found.");
            }

            const string TeacherRole = "Teacher";
            bool isLecturer = role == TeacherRole;

            if (!isLecturer)
            {
                throw new InvalidOperationException("Manual addition is only allowed for the 'Teacher' role. Students must join using the invite code.");
            }


            var staffExists = await _context.DepartmentStaffs.AnyAsync(s => s.StaffId == memberUserId);
            if (!staffExists)
            {
                throw new KeyNotFoundException($"The User ID '{memberUserId}' is not a valid Staff or Lecturer ID in the system.");
            }

            // 4. Kiểm tra thành viên đã tồn tại trong lớp
            var exists = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                               (m.LecturerId == memberUserId || m.StudentId == memberUserId));

            if (exists) return false; // Trả về false nếu thành viên đã tồn tại

            var member = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = classroomId,
                RoleInClass = role,
                LecturerId = memberUserId,
                StudentId = null,
            };
            await _context.ClassroomMembers.AddAsync(member);
            await _context.SaveChangesAsync();
            return true;
        }

        //==========Service to update role==========
        public async Task<bool> UpdateMemberRoleAsync (string classroomId, string userId, string newRole)
        {
            const string TeacherRole = "Teacher";
            bool isLecturer = newRole == TeacherRole;

            var member = await _context.ClassroomMembers
                .FirstOrDefaultAsync(m => m.ClassroomId == classroomId &&
                                          ((m.LecturerId == userId)|| (m.StudentId == userId)));

            if (member == null) return false;

            if (member.RoleInClass == newRole)
            {
                return true; // Không cần cập nhật, coi như thành công
            }

            member.RoleInClass = newRole;
            member.LecturerId = isLecturer ? userId : null;
            member.StudentId = isLecturer? null : userId;

            _context.ClassroomMembers.Update(member);
            await _context.SaveChangesAsync();
            return true;
        }

        //Service to get overview of classroom
        public async Task<ClassroomOverviewDto> GetOverviewAsync (string classroomId, string userId, string role)
        {
            string? businessId = null;
            if (role == "Lecturer" || role == "Staff" || role == "Admin")
            {
                // Tra cứu StaffId (Username) từ bảng StaffDeparts bằng UserId (UUIDv7)
                businessId = await _context.DepartmentStaffs
                    .Where(s => s.UserId== userId)
                    .Select(s => s.StaffId)
                    .FirstOrDefaultAsync();
            }
            // Nếu là Student, có thể StudentId cũng lưu UUIDv7 (userId), nên ta dùng luôn userId
            else if (role == "Student")
            {
                // Giả sử cột StudentId trong ClassroomMembers đang lưu UUIDv7
                businessId = userId;
            }
            var classroom = await _context.Classrooms
                .Where(c => c.ClassroomId == classroomId)
                .Select(c => new
                {
                    c.ClassroomId,
                    c.ClassName,
                    c.Description,
                    c.InviteCode,
                    c.ClassStatus,

                    ClassSubName = c.ClassSubNavigation.SubName,

                    LecturerFirstName = c.CreateByNavigation.FirstName,
                    LecturerLastName = c.CreateByNavigation.LastName,

                    IsTeacher = c.ClassroomMembers.Any(cm => cm.LecturerId == businessId && cm.RoleInClass == "Teacher"),
                    IsStudent = c.ClassroomMembers.Any(cm => cm.StudentId == businessId && cm.RoleInClass == "Student"),

                })
                .FirstOrDefaultAsync();

            if (classroom == null)
                return null;

            var lesson = await _context.Lessons
                .Where(l => l.ClassroomId == classroomId)
                .Select(l => new LessonViewDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    CreatBy = l.CreateByNavigation.FirstName + " " + l.CreateByNavigation.LastName,
                    CreateAt = l.CreateAt,
                }).ToListAsync();

            var assignment = await _context.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .Select(a => new AssignmentViewDto
                {
                    AssignId = a.AssignId,
                    Title = a.Title,
                    CreateBy = a.Teacher.FirstName + " " + a.Teacher.LastName,
                    Deadline = a.Deadline,
                    DeadlineStatus = a.Deadline > DateTime.UtcNow ? "Valid" : "Overdue"

                }).ToListAsync();
            // 4. Áp dụng Logic ẩn/hiện InviteCode
            string? inviteCodeForUser = null;

            // Nếu là Admin HOẶC (Lecturer/Staff VÀ là Teacher của lớp)
            if (role == "Admin" || classroom.IsTeacher || role == "Staff")
            {
                inviteCodeForUser = classroom.InviteCode;
            }

            return new ClassroomOverviewDto
            {
                ClassroomId = classroom.ClassroomId,
                ClassName = classroom.ClassName,
                ClassSub = classroom.ClassSubName,
                LecturerName = classroom.LecturerFirstName + " " + classroom.LecturerLastName,
                Description = classroom.Description,
                Status = classroom.ClassStatus,

                InviteCode = inviteCodeForUser, // Sẽ là null nếu người dùng là Student

                Lessons = lesson,
                Assignments = assignment
            };
        }

        //==========SERVICE TO UPDATE CLASSROOM INFORMATION==========
        public async Task<bool> UpdateClassroomAsync(string classroomId, UpdateClassroomDto dto, string staffId)
        {
            var classroom = await _context.Classrooms
                .AsTracking()
                .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);

            if (classroom == null)
                return false;

            //===Check day 14 day to block 
            var dayDiff = (DateTime.UtcNow - classroom.CreateDate)?.TotalDays ?? 0;
            bool isLocked = dayDiff > 14;

            //===update ClassName and Description (IF not block)
            if (!isLocked)
            {
                if (!string.IsNullOrWhiteSpace(dto.ClassName))
                    classroom.ClassName = dto.ClassName.Trim();

                if (dto.Description != null)
                    classroom.Description = dto.Description.Trim();
            }

            //==Update Status
            if (!string.IsNullOrWhiteSpace(dto.ClassStatus))
            {
                classroom.ClassStatus = dto.ClassStatus.Trim();
            }

            //===Write Activiti Log
            try
            {
                var departId = await _context.StaffDeparts
                    .Where(s => s.StaffId == staffId)
                    .Select(s => s.DepartId)
                    .FirstOrDefaultAsync();

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    DepartId = await _context.StaffDeparts
                        .Where(x => x.StaffId == staffId)
                        .Select(x => x.DepartId)
                        .FirstOrDefaultAsync(),
                    Action = "Update Classrooms",
                    TargetTable = "Classrooms",
                    TargetId = classroom.ClassroomId,
                    TargetName = classroom.ClassName,
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);
            }catch (Exception ex)
            {
                
            }
            await _context.SaveChangesAsync();
            return true;
        }

        //==========SERVICE TO DELETE CLASSROOM==========
        public async Task<bool> DeleteClassroomAsync(string classroomId, string staffId, string webRootPath)
        {
            //===Load classroom
            var classroom = await _context.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);

            if (classroom == null) 
                return false;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                //Delete all lesson + files
                var lessons = await _context.Lessons
                    .Include(l => l.LessonFiles)
                    .Where(l => l.ClassroomId == classroomId)
                    .ToListAsync();

                foreach (var lesson in lessons)
                {
                    var lessonFolder = Path.Combine(webRootPath, "uploads", "Lessons", classroomId, lesson.LessonId);

                    //delete lesson file
                    foreach (var file in lesson.LessonFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file.FilePath);
                            var filePath = Path.Combine(lessonFolder, fileName);

                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                        catch { }
                    }

                    //delete assignment folder
                    try
                    {
                        if (Directory.Exists(lessonFolder))
                            Directory.Delete(lessonFolder, true);
                    }
                    catch { }
                    _context.LessonFiles.RemoveRange(lesson.LessonFiles);
                    _context.Lessons.Remove(lesson);
                }

                //Delete Assignment + File + Submit
                var assignments = await _context.Assignments
                    .Include(a => a.AssignmentFiles)
                    .Where(a => a.ClassroomId == classroomId)
                    .ToListAsync();
                
                foreach(var assign in assignments)
                {
                    var assignFolder = Path.Combine(webRootPath, "uploads", "Assignments", classroomId, assign.AssignId);

                    //Delete Assignment Files
                    foreach(var file in assign.AssignmentFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file.FilePath);
                            var filePath = Path.Combine(assignFolder, fileName);

                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                        catch { }
                    }

                    //Delete assginment folder
                    try
                    {
                        if (Directory.Exists(assignFolder))
                            Directory.Delete(assignFolder, true);
                    }
                    catch { }

                    //Delete submission
                    var submissions = await _context.Submissions
                        .Where(s => s.AssignId == assign.AssignId)
                        .ToListAsync();

                    _context.Submissions.RemoveRange(submissions);

                    //Delete db record
                    _context.AssignmentFiles.RemoveRange(assign.AssignmentFiles);
                    _context.Assignments.Remove(assign);
                }

                //Delete members
                var members = await _context.ClassroomMembers
                    .Where(m => m.ClassroomId == classroomId)
                    .ToListAsync();
                _context.ClassroomMembers.RemoveRange(members);

                //DELETE CLASSROOM
                _context.Classrooms.Remove(classroom);

                //WRITE log
                try
                {
                    await _context.ActivityLogs.AddAsync(new ActivityLog
                    {
                        LogId = Uuidv7Generator.NewUuid7().ToString(),
                        StaffId = staffId,
                        DepartId = _context.StaffDeparts
                            .Where (s => s.StaffId == staffId)
                            .Select(s => s.DepartId)
                            .FirstOrDefault(),
                        Action = "Delete Classroom",
                        TargetId = classroomId,
                        TargetTable = "Classrooms",
                        TargetName = classroom.ClassName,
                        Timestap = DateTime.UtcNow
                    });
                }
                catch { }

                //Save chage
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return true;

            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

        }

    }
}
