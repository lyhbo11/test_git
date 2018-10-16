using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obis_Sample.Patients
{
    public class Patient
    {
        public string Patient_sex { get; set; }
        public string Patient_name { get; set; }
        public string Patient_age { get; set; }
        public string Patient_ID { get; set; }
    }
    public class PatientsDetial
        {
            public string Patient_sex { get; set; }
            public string Patient_name { get; set; }
            public string Patient_age { get; set; }
            public string Patient_ID { get; set; }

            public string Timetoget { get; set; }
            public string Patient_RightSight { get; set; }
            public string Patient_LeftSight { get; set; }
            public string Patient_Record { get; set; }
            public string Patient_OtherDis { get; set; }
        }
    public class PatientForJson
    {
        public string patient_sex { get; set; }
        public string patient_name { get; set; }
        public double patient_age { get; set; }
        public string patient_ID { get; set; }
    }
}
