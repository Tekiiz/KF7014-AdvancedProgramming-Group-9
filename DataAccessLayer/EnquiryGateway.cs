﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IEnquiryGateway
    {
        bool SaveEnquiry(Enquiry enquiry, Customer customer);
        List<Enquiry> GetAllEnquiries();
        Enquiry GetEnquiry(int id);
        bool DeleteEnquiry(int id);
        bool DeleteAllEnquiries();
        bool UpdateEnquiry(Enquiry enquiry);
        //this is in response to the data being duplicated.
        bool SaveEnquiryAll(Enquiry enquiry, List<OrderItems> orderItems, Customer customer);
    }
    public class EnquiryGateway : IEnquiryGateway
    {
        public bool SaveEnquiry(Enquiry enquiry, Customer customer)
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    //enquiry.Customer = customer;
                    context.Enquiries.Add(enquiry);
                    context.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        //I'm not 100% happy with this method as it defeats the purpose of the other gateways, but otherwise all the data will be duplicated.
        public bool SaveEnquiryAll(Enquiry enquiry, List<OrderItems> orderItems, Customer customer)
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    enquiry.Customer = customer;
                    context.Enquiries.Add(enquiry);
                    context.Customers.Add(customer);

                    for (int i = 0; i < orderItems.Count(); i++)
                    {
                        //adds all the of the items in the order to a database
                        orderItems[i].Enquiry = enquiry;
                        context.OrderItems.Add(orderItems[i]);
                    }

                    context.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public Enquiry GetEnquiry(int id)
        {
            try
            {
                //based on code from https://docs.microsoft.com/en-us/ef/core/querying/
                using (var context = new DatabaseEntities())
                {
                    var enquiryQuery = context.Enquiries.Where(e => e.orderID == id).SingleOrDefault();
                    return enquiryQuery;
                }
            }
            catch { return new Enquiry(); }
        }

        public List<Enquiry> GetAllEnquiries()
        {
            try
            {
                //based on code from https://docs.microsoft.com/en-us/ef/core/querying/
                using (var context = new DatabaseEntities())
                {
                    var enquiryQuery = context.Enquiries.ToList();
                    return enquiryQuery;
                }
            }

            catch { return new List<Enquiry>(); }
        }

        public bool UpdateEnquiry(Enquiry enquiry)
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    //based on code from https://docs.microsoft.com/en-us/ef/core/querying/
                    var orderItemsQuery = context.Enquiries.Where(i => i.orderID == enquiry.orderID).SingleOrDefault();
                    orderItemsQuery.deadline = enquiry.deadline;
                    orderItemsQuery.price = enquiry.price;
                    orderItemsQuery.hoursToComplete = enquiry.hoursToComplete;
                    orderItemsQuery.orderStatus = enquiry.orderStatus;
                    orderItemsQuery.orderNotes = enquiry.orderNotes;
                    context.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public bool DeleteEnquiry(int id)
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    var enquiryGetQuery = context.Enquiries.Where(e => e.orderID == id).SingleOrDefault();
                    var enquiryQuery = context.Enquiries.Remove(enquiryGetQuery);
                    context.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }
        public bool DeleteAllEnquiries()
        {
            try
            {
                using (var context = new DatabaseEntities())
                {
                    var enquiryGetQuery = context.Enquiries.ToList();

                    foreach (Enquiry e in enquiryGetQuery)
                    {
                        var enquiryQuery = context.Enquiries.Remove(e);
                    }

                    context.SaveChanges();

                    var newEnquiryGetQuery = context.Enquiries.ToList();
                    if (newEnquiryGetQuery.Count() == 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch { return false; }
        }
    }
}