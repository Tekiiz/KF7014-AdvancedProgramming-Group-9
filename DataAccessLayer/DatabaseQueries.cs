﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IDatabaseCreateQueries
    {
        bool SaveEnquiry(Enquiry enquiry);
        void SaveCustomer(Customer customer);
    }

    public interface IDatabaseReadQueries
    {
        List<Enquiry> GetAllEnquiries();
        Enquiry GetEnquiry(int id);
    }


    //spliting the datebase class into seperate classes for CRUD operations.
    public class DatabaseCreateQueries : IDatabaseCreateQueries
    {
        public bool SaveEnquiry(Enquiry enquiry)
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    context.Enquiry.Add(enquiry);

                    /*()
                    for (int i = 0; i < enquiry.countItemInOrder(); i++)
                    {
                        //adds all the of the items in the order to a database
                        context.OrderItems.Add(enquiry.getItemInOrder(i));
                    }
                    */

                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void SaveCustomer(Customer customer)
        {
            using (var context = new DatabaseEntities())
            {
                context.Customer.Add(customer);
                context.SaveChanges();
            }
        }
    }

    public class DatabaseReadQueries
    {
        public Enquiry GetEnquiry(int id)
        {
            //based on code from https://docs.microsoft.com/en-us/ef/core/querying/
            using (var context = new DatabaseEntities())
                {
                    var enquiryQuery = context.Enquiry.Where(e => e.orderID == id).SingleOrDefault();
                    return enquiryQuery;
                }
        }

        public List<Enquiry> GetAllEnquiries()
        {
            //based on code from https://docs.microsoft.com/en-us/ef/core/querying/
            using (var context = new DatabaseEntities())
            {
                var enquiryQuery = context.Enquiry.ToList();
                return enquiryQuery;
            }
        }
    }

    public class DatabaseUpdateQueries
    {
        //blank
    }

    public class DatabaseDeleteQueries
    {
        //blank
    }
}