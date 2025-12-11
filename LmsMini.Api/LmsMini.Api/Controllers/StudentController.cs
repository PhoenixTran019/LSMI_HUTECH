using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using LmsMini.Application.Common.Helpers;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LmsMini.Application.DTOs.Student;
using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Application.DTOs.Assignment;
using LmsMini.Application.DTOs.Lesson;


namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly LmsDbContext _context;

        public StudentController(IStudentService studentService, LmsDbContext context)
        {
            _studentService = studentService;
            _context = context;
        }

        // ============================================================
        // 1. STAFF / TRAINING MANAGER: Tạo student + account đăng nhập
        // ============================================================
        [AllowAnonymous]
        [Authorize(Roles = "TrainingManager,Admin")]
        [HttpPost("create-student")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Cannot identify current staff user.");
            }

            var staff = await _context.DepartmentStaffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null)
            {
                return BadRequest("Staff account not found.");
            }

            var success = await _studentService
                .CreateStudentWithAccountAsync(dto, staff.StaffId);

            if (!success)
            {
                return BadRequest("Student ID already exists or Student role has not been configured.");
            }

            return Ok("Create Student and Student Account Success!");
        }

        // ============================================================
        // 2. STUDENT – PROFILE
        // ============================================================

        // 2.1 Hồ sơ của chính mình
        [Authorize(Roles = "Student")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            Class? @class = null;
            Department? depart = null;
            Major? major = null;

            if (!string.IsNullOrEmpty(student.ClassId))
            {
                @class = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassId == student.ClassId);
            }

            if (!string.IsNullOrEmpty(student.DepartId))
            {
                depart = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartId == student.DepartId);
            }

            if (!string.IsNullOrEmpty(student.StuMajor))
            {
                major = await _context.Majors
                    .FirstOrDefaultAsync(m => m.MajorId == student.StuMajor);
            }

            var dto = new StudentProfileDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,

                ClassId = student.ClassId,
                ClassName = @class?.ClassName,

                DepartId = student.DepartId,
                DepartName = depart?.DepartName,

                StuMajor = student.StuMajor,
                MajorName = major?.MajorName,

                FirstName = student.FirstName,
                LastName = student.LastName,
                Gender = student.Gender,

                PhoneNum = student.PhoneNum,
                Mail = student.Mail,
                Address = student.Address,
                Image = student.Image
            };

            return Ok(dto);
        }

        // 2.2 Cập nhật thông tin liên lạc
        [Authorize(Roles = "Student")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            if (dto.PhoneNum != null) student.PhoneNum = dto.PhoneNum;
            if (dto.Mail != null) student.Mail = dto.Mail;
            if (dto.Address != null) student.Address = dto.Address;
            if (dto.Image != null) student.Image = dto.Image;

            await _context.SaveChangesAsync();
            return Ok("Update profile success.");
        }

        // ============================================================
        // 3. STUDENT – CLASSROOM (LỚP MÔN HỌC)
        // ============================================================

        // 3.1 Danh sách classroom mà sinh viên đang tham gia
        [Authorize(Roles = "Student")]
        [HttpGet("my-classrooms")]
        public async Task<IActionResult> GetMyClassrooms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(studentId))
                return NotFound("Student profile not found.");

            var query =
                from member in _context.ClassroomMembers
                join classroom in _context.Classrooms
                    on member.ClassroomId equals classroom.ClassroomId
                join subject in _context.Subjects
                    on classroom.ClassSub equals subject.SubId
                where member.StudentId == studentId
                select new StudentClassroomDto
                {
                    ClassroomId = classroom.ClassroomId,
                    ClassroomName = classroom.ClassName,
                    SubjectId = subject.SubId,
                    SubjectName = subject.SubName,
                    SubjectCode = subject.SubCode,
                    InviteCode = classroom.InviteCode,
                    ClassStatus = classroom.ClassStatus
                };

            var list = await query.ToListAsync();
            return Ok(list);
        }

        // 3.2 Join classroom bằng InviteCode (lớp môn học)
        [Authorize(Roles = "Student")]
        [HttpPost("classroom/join-by-invite")]
        public async Task<IActionResult> JoinClassroom([FromBody] JoinClassroomDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.InviteCode))
                return BadRequest("InviteCode is required.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.InviteCode == dto.InviteCode);

            if (classroom == null)
                return NotFound("Classroom with this invite code not found.");

            var exists = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == classroom.ClassroomId &&
                m.StudentId == student.StudentId);

            if (exists)
                return BadRequest("You are already a member of this classroom.");

            var member = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = classroom.ClassroomId,
                StudentId = student.StudentId,
                RoleInClass = "Student"
            };

            await _context.ClassroomMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            return Ok("Join classroom success.");
        }

        // ============================================================
        // 4. STUDENT – LESSON (bài giảng trong classroom)
        // ============================================================

        // 4.1 Danh sách lesson trong một classroom
        [Authorize(Roles = "Student")]
        [HttpGet("classrooms/{classroomId}/lessons")]
        public async Task<IActionResult> GetLessonsInClassroom(string classroomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(studentId))
                return NotFound("Student profile not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == classroomId &&
                m.StudentId == studentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var lessons = await _context.Lessons
                .Where(l => l.ClassroomId == classroomId)
                .OrderBy(l => l.CreateAt)
                .Select(l => new StudentLessonListItemDto
                {
                    LessonId = l.LessonId,
                    ClassroomId = l.ClassroomId,
                    Title = l.Title,
                    CreateAt = l.CreateAt,
                    FileCount = _context.LessonFiles.Count(f => f.LessonId == l.LessonId)
                })
                .ToListAsync();

            return Ok(lessons);
        }

        // 4.2 Chi tiết một lesson (kèm file)
        [Authorize(Roles = "Student")]
        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetLessonDetailForStudent(string lessonId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
                return NotFound("Lesson not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == lesson.ClassroomId &&
                m.StudentId == student.StudentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var files = await _context.LessonFiles
                .Where(f => f.LessonId == lesson.LessonId)
                .Select(f => new StudentLessonFileDto
                {
                    LessonFileId = f.FilesId,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType,
                    UploadAt = f.UpdateAt
                })
                .ToListAsync();

            var dto = new StudentLessonDetailDto
            {
                LessonId = lesson.LessonId,
                ClassroomId = lesson.ClassroomId,
                Title = lesson.Title,
                Content = lesson.Content,
                CreateAt = lesson.CreateAt,
                Files = files
            };

            return Ok(dto);
        }

        // ============================================================
        // 5. STUDENT – ASSIGNMENT (bài tập)
        // ============================================================

        // 5.1 Danh sách assignment trong một classroom
        [Authorize(Roles = "Student")]
        [HttpGet("classrooms/{classroomId}/assignments")]
        public async Task<IActionResult> GetAssignmentsInClassroom(string classroomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(studentId))
                return NotFound("Student profile not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == classroomId &&
                m.StudentId == studentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var assignments = await _context.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .OrderBy(a => a.Deadline)
                .Select(a => new StudentAssignmentListItemDto
                {
                    AssignmentId = a.AssignId,
                    ClassroomId = a.ClassroomId,
                    Title = a.Title,
                    Deadline = a.Deadline,
                    DeadlineStatus = a.DeadlineStatus,
                    HomeworkStatus = a.HomeworkStatus,
                    IsSubmitted = _context.Submissions
                        .Any(s => s.AssignId == a.AssignId && s.StudentId == studentId),
                    SubmittedAt = _context.Submissions
                        .Where(s => s.AssignId == a.AssignId && s.StudentId == studentId)
                        .Select(s => s.SubmitAt)
                        .FirstOrDefault(),
                    Grade = _context.Submissions
                        .Where(s => s.AssignId == a.AssignId && s.StudentId == studentId)
                        .Select(s => s.Grade)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // 5.2 Chi tiết assignment cho student
        [Authorize(Roles = "Student")]
        [HttpGet("assignments/{assignmentId}")]
        public async Task<IActionResult> GetAssignmentDetailForStudent(string assignmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == assignment.ClassroomId &&
                m.StudentId == student.StudentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var files = await _context.AssignmentFiles
                .Where(f => f.AssignId == assignment.AssignId)
                .Select(f => new StudentAssignmentFileDto
                {
                    AssignmentFileId = f.FileId,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType,
                    UploadAt = null // bảng AssignmentFiles hiện không có cột time
                })
                .ToListAsync();

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignId == assignment.AssignId &&
                                          s.StudentId == student.StudentId);

            var dto = new StudentAssignmentDetailDto
            {
                AssignmentId = assignment.AssignId,
                ClassroomId = assignment.ClassroomId,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                DeadlineStatus = assignment.DeadlineStatus,
                HomeworkStatus = assignment.HomeworkStatus,
                Files = files,
                IsSubmitted = submission != null,
                SubmittedAt = submission?.SubmitAt,
                Grade = submission?.Grade,
                Feedback = submission?.FeedBack
            };

            return Ok(dto);
        }

        // 5.3 Nộp bài assignment (upload file)
        [Authorize(Roles = "Student")]
        [HttpPost("assignments/{assignmentId}/submit")]
        public async Task<IActionResult> SubmitAssignment(
            string assignmentId,
            [FromForm] List<IFormFile> files)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == assignment.ClassroomId &&
                m.StudentId == student.StudentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var existing = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignId == assignment.AssignId &&
                                          s.StudentId == student.StudentId);

            if (existing != null)
                return BadRequest("You have already submitted this assignment.");

            if (files == null || files.Count == 0)
                return BadRequest("At least one file is required.");

            var submitId = Uuidv7Generator.NewUuid7().ToString();

            var submission = new Submission
            {
                SubmitId = submitId,
                AssignId = assignment.AssignId,
                StudentId = student.StudentId,
                SubmitAt = DateTime.UtcNow,
                FeedBack = null,
                Grade = null
                // Không gán SubmitType nữa vì entity không có property này
            };

            await _context.Submissions.AddAsync(submission);

            // Lưu file vào wwwroot/submissions/{AssignId}/{StudentId}
            var rootPath = Directory.GetCurrentDirectory();
            var uploadFolder = Path.Combine(
                rootPath,
                "wwwroot",
                "submissions",
                assignment.AssignId,
                student.StudentId);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var extension = Path.GetExtension(file.FileName);
                var newFileName = $"{Guid.NewGuid()}{extension}";
                var savePath = Path.Combine(uploadFolder, newFileName);

                await using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine(
                    "submissions",
                    assignment.AssignId,
                    student.StudentId,
                    newFileName).Replace("\\", "/");

                var submitFile = new SubmitFile
                {
                    FileId = Uuidv7Generator.NewUuid7().ToString(),
                    SubmitId = submitId,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    FileType = file.ContentType,
                    UpdateAt = DateTime.UtcNow
                };

                await _context.SubmitFiles.AddAsync(submitFile);
            }

            await _context.SaveChangesAsync();

            return Ok("Submit assignment success.");
        }


        // 5.4 Xem bài nộp của chính mình cho một assignment
        [Authorize(Roles = "Student")]
        [HttpGet("assignments/{assignmentId}/submission")]
        public async Task<IActionResult> GetMySubmission(string assignmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify current user.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            var isMember = await _context.ClassroomMembers.AnyAsync(m =>
                m.ClassroomId == assignment.ClassroomId &&
                m.StudentId == student.StudentId);

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignId == assignment.AssignId &&
                                          s.StudentId == student.StudentId);

            if (submission == null)
                return NotFound("You have not submitted this assignment yet.");

            var files = await _context.SubmitFiles
                .Where(f => f.SubmitId == submission.SubmitId)
                .Select(f => new StudentSubmissionFileDto
                {
                    SubmitFileId = f.FileId,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType,
                    UploadAt = f.UpdateAt
                })
                .ToListAsync();

            var dto = new StudentSubmissionDto
            {
                SubmitId = submission.SubmitId,
                AssignmentId = submission.AssignId,
                SubmitAt = submission.SubmitAt,
                Feedback = submission.FeedBack,
                Grade = submission.Grade,
                Files = files
            };

            return Ok(dto);
        }

        // ============================================================
        // 6. STUDENT – PROJECT CLASSROOM (lớp đồ án, đã làm trước đó)
        // ============================================================

        // Join lớp đồ án cho CẢ NHÓM bằng InviteCode (ProClassId)
        [Authorize(Roles = "Student")]
        [HttpPost("project-classroom/join-by-invite")]
        public async Task<IActionResult> JoinProjectClassroom([FromBody] JoinProjectClassroomDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.InviteCode) ||
                string.IsNullOrWhiteSpace(dto.StudentID))
            {
                return BadRequest("InviteCode and StudentID are required.");
            }

            var projectClassroom = await _context.ProjectClassrooms
                .FirstOrDefaultAsync(pc =>
                    pc.ProClassId == dto.InviteCode && pc.IsActive == true);

            if (projectClassroom == null)
            {
                return NotFound("Project classroom not found or not active.");
            }

            var activeAssign = await (
                from mem in _context.ProjectMenbers
                join assign in _context.ProjectAssigns
                    on mem.RegistId equals assign.AssignId
                where mem.StudentId == dto.StudentID
                      && (assign.Status == "Active" || assign.Status == "Approved")
                select assign
            ).FirstOrDefaultAsync();

            if (activeAssign == null)
            {
                return BadRequest("Student does not have any active/approved project assignment.");
            }

            bool alreadyJoined = await _context.ProjectClassMems.AnyAsync(pcMem =>
                pcMem.ProClassId == projectClassroom.ProClassId &&
                pcMem.AssignId == activeAssign.AssignId);

            if (alreadyJoined)
            {
                var existedMembers = await GetProjectMembersOfAssign(activeAssign.AssignId);

                return Ok(new
                {
                    Message = "Group has already joined this project classroom.",
                    ProjectClassId = projectClassroom.ProClassId,
                    AssignId = activeAssign.AssignId,
                    Members = existedMembers
                });
            }

            var newClassMem = new ProjectClassMem
            {
                ProClassMemId = Uuidv7Generator.NewUuid7().ToString(),
                ProClassId = projectClassroom.ProClassId,
                AssignId = activeAssign.AssignId,
                LecturerId = null,
                RoleInClass = "Student"
            };

            await _context.ProjectClassMems.AddAsync(newClassMem);
            await _context.SaveChangesAsync();

            var members = await GetProjectMembersOfAssign(activeAssign.AssignId);

            return Ok(new
            {
                Message = "Nhóm đã tham gia lớp đồ án thành công.",
                ProjectClassId = projectClassroom.ProClassId,
                AssignId = activeAssign.AssignId,
                Members = members
            });
        }

        // Lấy danh sách thành viên của một AssignId (dùng nội bộ)
        private async Task<System.Collections.Generic.List<object>> GetProjectMembersOfAssign(string assignId)
        {
            var members = await (
                from mem in _context.ProjectMenbers
                join stu in _context.Students
                    on mem.StudentId equals stu.StudentId
                where mem.RegistId == assignId
                select new
                {
                    stu.StudentId,
                    FullName = stu.FirstName + " " + stu.LastName,
                    stu.Mail
                }
            ).ToListAsync();

            return members.Cast<object>().ToList();
        }
    }
}
