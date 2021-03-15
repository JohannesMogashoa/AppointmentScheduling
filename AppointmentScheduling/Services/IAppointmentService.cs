using AppointmentScheduling.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduling.Services
{
    public interface IAppointmentService
    {
        public List<DoctorViewModel> GetDoctorList();
        public List<PatientViewModel> GetPatientList();
        public Task<int> AddUpdate(AppointmentViewModel model);

        public List<AppointmentViewModel> DoctorsEventsById(string doctorId);
        public List<AppointmentViewModel> PatientsEventsById(string patientId);

        public AppointmentViewModel GetById(int id);

        public Task<int> Delete(int id);

        public Task<int> ConfirmEvent(int id);
    }
}
