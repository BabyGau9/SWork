

namespace SWork.Repository.Repository
{
    public class InterviewReposiory(SWorkDbContext context) : GenericRepository<Interview>(context), IInterviewRepository
    {
        public async Task<Interview> GetByIdAsync(int id)
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                        .ThenInclude(j => j.Employer)
                            .ThenInclude(e => e.User)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(i => i.InterviewID == id);
        }

        public async Task<IEnumerable<Interview>> GetAllAsync()
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                        .ThenInclude(j => j.Employer)
                            .ThenInclude(e => e.User)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Interview>> GetByApplicationIdAsync(int applicationId)
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                        .ThenInclude(j => j.Employer)
                            .ThenInclude(e => e.User)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .Where(i => i.ApplicationID == applicationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Interview>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                        .ThenInclude(j => j.Employer)
                            .ThenInclude(e => e.User)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .Where(i => i.Application.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Interview>> GetByEmployerIdAsync(int employerId)
        {
            return await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Job)
                        .ThenInclude(j => j.Employer)
                            .ThenInclude(e => e.User)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.User)
                .Where(i => i.Application.Job.EmployerID == employerId)
                .ToListAsync();
        }

        public async Task<Interview> AddAsync(Interview interview)
        {
            await _context.Interviews.AddAsync(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<Interview> UpdateAsync(Interview interview)
        {
            _context.Interviews.Update(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null)
                return false;

            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
