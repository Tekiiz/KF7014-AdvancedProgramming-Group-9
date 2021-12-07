﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// public interface 

namespace Group9Assignment
{
    public class Customer
    {
        public int customerID { get; set; }
        public string firstNames { get; set; }
        public string middleNames { get; set; }
        public string lastNames { get; set; }
        public string companyNames { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string postcode { get; set; }
        public string townCity { get; set; }
        public string stateCounty { get; set; }
        public string country { get; set; }
        public int phone { get; set; }
        public string email { get; set; }
        public string bankdetails { get; set; }
        public int balance { get; set; }
    }
}