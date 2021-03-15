using AppointmentScheduling.Data;
using AppointmentScheduling.Models;
using AppointmentScheduling.Models.ViewModels;
using AppointmentScheduling.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduling.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        public AppointmentService(ApplicationDbContext dbContext, IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
        }

        public async Task<int> AddUpdate(AppointmentViewModel model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));
            var patient = _dbContext.Users.FirstOrDefault(p => p.Id == model.PatientId);
            var doctor = _dbContext.Users.FirstOrDefault(d => d.Id == model.DoctorId);

            if(model != null && model.Id > 0)
            {
                //update
                var appointment = _dbContext.Appointments.FirstOrDefault(a => a.Id == model.Id);

                appointment.Title = model.Title;
                appointment.Description = model.Description;
                appointment.StartDate = startDate;
                appointment.EndDate = endDate;
                appointment.Duration = model.Duration;
                appointment.DoctorId = model.DoctorId;
                appointment.PatientId = model.PatientId;
                appointment.IsDoctorApproved = false;
                appointment.AdminId = model.AdminId;

                await _dbContext.SaveChangesAsync();
                return 1;
            }
            else
            {
                //create
                Appointment appointment = new Appointment()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = model.Duration,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    IsDoctorApproved = false,
                    AdminId = model.AdminId
                };

                await _emailSender.SendEmailAsync(doctor.Email, "Appointment Created", $"Your appointment with {patient.Name} is created and is in pending status");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Created", $"Your appointment with {doctor.Name} is created and is in pending status");
                _dbContext.Appointments.Add(appointment);
                await _dbContext.SaveChangesAsync();
                return 2;
            }
        }

        public async Task<int> ConfirmEvent(int id)
        {
            var appointment = _dbContext.Appointments.Where(a => a.Id.Equals(id)).FirstOrDefault();
            if(appointment != null)
            {
                appointment.IsDoctorApproved = true;
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> Delete(int id)
        {
            var appointment = _dbContext.Appointments.Where(a => a.Id.Equals(id)).FirstOrDefault();
            if (appointment != null)
            {
                _dbContext.Appointments.Remove(appointment);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public List<AppointmentViewModel> DoctorsEventsById(string doctorId)
        {
            return _dbContext.Appointments.Where(d => d.DoctorId == doctorId).ToList().Select(c => new AppointmentViewModel()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved
            }).ToList();
        }

        public AppointmentViewModel GetById(int id)
        {
            return _dbContext.Appointments.Where(a => a.Id == id).ToList().Select(c => new AppointmentViewModel()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved,
                DoctorId = c.DoctorId,
                PatientId = c.PatientId,
                PatientName = _dbContext.Users.Where(pN => pN.Id == c.PatientId).Select(p => p.Name).FirstOrDefault(),
                DoctorName = _dbContext.Users.Where(dN => dN.Id == c.DoctorId).Select(d => d.Name).FirstOrDefault()
            }).FirstOrDefault();
        }

        public List<DoctorViewModel> GetDoctorList()
        {
            var doctors = (from user in _dbContext.Users
                           join userRoles in _dbContext.UserRoles on user.Id equals userRoles.UserId
                           join roles in _dbContext.Roles.Where(r => r.Name==Helper.Doctor) on userRoles.RoleId equals roles.Id
                           select new DoctorViewModel
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();
            return doctors;
        }

        public List<PatientViewModel> GetPatientList()
        {
            var patients = (from user in _dbContext.Users
                           join userRoles in _dbContext.UserRoles on user.Id equals userRoles.UserId
                           join roles in _dbContext.Roles.Where(r => r.Name == Helper.Patient) on userRoles.RoleId equals roles.Id
                           select new PatientViewModel
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();
            return patients;
        }

        public List<AppointmentViewModel> PatientsEventsById(string patientId)
        {
            return _dbContext.Appointments.Where(d => d.PatientId == patientId).ToList().Select(c => new AppointmentViewModel()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsDoctorApproved = c.IsDoctorApproved
            }).ToList();
        }
    }
}
