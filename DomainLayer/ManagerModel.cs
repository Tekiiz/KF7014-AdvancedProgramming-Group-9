﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace DomainLayer
{
    /*
     * This model is used to cover functional requirement #2
     * 
     * 2.Production estimates: The contracts manager will quickly assess the time needed to complete the order this will be in hours, 
     * and price the work accordingly. The system will then check to see if the work can be scheduled.
     * 
     */

    public interface IManagerModel
    {
        List<OrderItems> GetItemsInEnquiry(int enquiryID);
        List<Enquiry> GetEnquiries();
        Enquiry GetEnquiry(int enquiryID);
        Customer GetCustomerInEnquiry(int customerID);
        void UpdateEnquiry(Enquiry enquiry);
        bool UpdateOrder(Order order);
        int GetEnquiryInOrder(Order order);
        void CalculateEstimatedTime(out int minTime, out int maxTime, out double minCost, out double maxCost, List<OrderItems> orderItems);
        bool CheckSchedule(DateTime checkStartDate, DateTime checkDeadline);
        bool PriceHoursCheck(Double price, int hours, List<OrderItems> orderItems);
        bool SaveOrder(Order order, Enquiry enquiry);
        bool CheckIfDeadlineIsFeasible(int hours, DateTime startDate, DateTime deadline);
        List<Order> canOrderBeMoved(Order replacementOrder, Customer customer);
        Order conflictingOrder(DateTime checkStartDate, DateTime checkDeadline);
        Order GetOrder(int id);
    }

    public class ManagerModel : IManagerModel
    {
        IEnquiryGateway enquiryCRUD;
        ICustomerGateway customerCRUD;
        IOrderItemGateway orderItemsCRUD;
        IOrderGateway orderCRUD;

        public ManagerModel(IEnquiryGateway enquiryCRUD, ICustomerGateway customerCRUD, IOrderItemGateway orderItemsCRUD, IOrderGateway orderCRUD)
        {
            this.enquiryCRUD = enquiryCRUD;
            this.customerCRUD = customerCRUD;
            this.orderItemsCRUD = orderItemsCRUD;
            this.orderCRUD = orderCRUD;
        }

        #region "Data Access Methods"
        public void UpdateEnquiry(Enquiry enquiry)
        {
            enquiryCRUD.UpdateEnquiry(enquiry);
        }

        public bool UpdateOrder(Order order)
        {
            try
            {
                orderCRUD.UpdateOrder(order);
                return true;
            }
            catch { return false; }
        }
        public bool SaveOrder(Order order, Enquiry enquiry)
        {
            try
            {
                order.Enquiry = enquiry;
                orderCRUD.SaveOrder(order);
                return true;
            }
            catch { return false; }
        }
        public List<Enquiry> GetEnquiries()
        {
            return enquiryCRUD.GetAllEnquiries();
        }

        public List<OrderItems> GetItemsInEnquiry(int enquiryID)
        {
            return orderItemsCRUD.GetOrderItemsInEnquiry(enquiryID);
        }

        public Enquiry GetEnquiry(int enquiryID)
        {
            return enquiryCRUD.GetEnquiry(enquiryID);
        }

        public Customer GetCustomerInEnquiry(int customerID)
        {
            try { return customerCRUD.GetCustomer(customerID); }
            catch { return new Customer(); }
        }

        public int GetEnquiryInOrder(Order order)
        {
            try { return orderCRUD.FindEnquiryIDinOrder(order); }
            catch { return 0; }
        }

        public Order GetOrder(int id)
        {
            try { return orderCRUD.GetOrder(id); }
            catch { return null; };
        }
        #endregion

        //This current version will be "dumb" - as in it it just checks an order against a time. -this can be changed later on.
        public bool CheckSchedule(DateTime checkStartDate, DateTime checkDeadline)
        {
            if (conflictingOrder(checkStartDate, checkDeadline) is null) { return true; }
            else { System.Windows.Forms.MessageBox.Show("There is already an order during that time period."); return false; }
        }

        public Order conflictingOrder(DateTime checkStartDate, DateTime checkDeadline)
        {
            foreach (Order order in orderCRUD.GetAllOrders())
            {
                DateTime orderStartDate = order.scheduledStartDate;
                int percentComplete = order.progressCompleted;
                DateTime orderDeadline = order.confirmedDeadline;
                DateTime dateWithin = checkStartDate;

                //look for all orders that are not completed and start before the deadline
                if (orderStartDate < checkDeadline && percentComplete < 100)
                {    
                    int days = (checkDeadline - checkStartDate).Days;

                    while (days > 0)
                    {     
                        //check to see if a date lands between the order start date and the deadline
                        if (dateWithin >= orderStartDate && dateWithin <= orderDeadline) { return order; }  
                        else { dateWithin = dateWithin.AddHours(24.0); days--;}
                    }
                }
            }          
            return null;
        }

        public List<Order> canOrderBeMoved(Order replacementOrder, Customer customer)
        {
            //this will be used to store the object
            List<Order> orderlist = new List<Order>();

            DateTime startDate = replacementOrder.scheduledStartDate;
            DateTime deadline = replacementOrder.confirmedDeadline;
            string customerType = customer.type;
            //collectors are the lowest priority (the order looking to replace)
            if (customerType.Equals("Collector")) { return orderlist; }
            //to be sure that the conflicting order is not null
            Order orderToBeReplaced =  conflictingOrder(startDate, deadline);
            int orderToBeReplacedID = orderCRUD.FindEnquiryIDinOrder(orderToBeReplaced);

            if (orderToBeReplaced is null) { return orderlist; }
            else 
            {
                Enquiry customerEnquiry = enquiryCRUD.GetEnquiry(orderToBeReplacedID);
                DateTime newStartDate = orderToBeReplaced.confirmedDeadline.AddHours(-customerEnquiry.hoursToComplete * 3);
                //collectors are the lowest priority (the order to be replaced)
                //based on "it appears as scheduled work but is should always be removed to make way for any orders" - count should be 1
                int customerID = enquiryCRUD.FindCustomerIDinEnquiry(customerEnquiry);
                if (customerCRUD.GetCustomer(customerID).type.Equals("Collector")) { orderlist.Add(orderToBeReplaced); orderlist.Add(replacementOrder); return orderlist; }
                //if it is possible to move the start date without moving the deadline (this doesn't account for the other order).
                else if (CheckIfDeadlineIsFeasible(customerEnquiry.hoursToComplete, newStartDate,
                    orderToBeReplaced.confirmedDeadline) == true)
                {
                    //assume a check has been performed so that the deadline and start date are feasible.
                    if (deadline < newStartDate)
                    {

                        System.Windows.Forms.MessageBox.Show(deadline.ToString());
                        System.Windows.Forms.MessageBox.Show(newStartDate.ToString());

                        orderToBeReplaced.scheduledStartDate = newStartDate;
                        //the orderToBeReplaced is always 0
                        //count should be 2
                        orderlist.Add(orderToBeReplaced);
                        orderlist.Add(replacementOrder);
                        return orderlist;
                    }
                }
                //count should be 0
                return orderlist;
            }
        }

        //checks to see if the start date is before the deadline and the hours needed added by the contracts manger would place the calculated deadline after the actual deadline.
        public bool CheckIfDeadlineIsFeasible(int hours, DateTime startDate, DateTime deadline)
        {
            //24 hours / 3 = 8.
            DateTime timeNeeded = startDate.AddHours(hours * 3);
            if (startDate > deadline) 
            { System.Windows.Forms.MessageBox.Show("Deadline is before start date."); return false; }
            else if (timeNeeded > deadline)
            { System.Windows.Forms.MessageBox.Show("Hours needed is greater than the deadline."); return false; }
            else { return true; }
        }

        public void CalculateEstimatedTime(out int minTime, out int maxTime, out double minCost, out double maxCost, List<OrderItems> orderItems)
        {
            minTime = 0;
            maxTime = 0;
            minCost = 0;
            maxCost = 0;

            for (int i = 0; i < orderItems.Count(); i++)
            {
                orderItems[i].getItemTime(out int minCalcTime, out int maxCalcTime);
                orderItems[i].getItemCost(out double minCalcCost, out double maxCalcCost);
                minTime += minCalcTime;
                maxTime += maxCalcTime;
                minCost += minCalcCost;
                maxCost += maxCalcCost;
            }
        }

        public bool PriceHoursCheck(Double price, int hours, List<OrderItems> orderItems)
        {
            CalculateEstimatedTime(out int minTime, out int maxTime, out double minCost, out double maxCost, orderItems);
            if (price < minCost || price > maxCost)
            {
                System.Windows.Forms.MessageBox.Show("Price should be between " + minCost.ToString() + " and " + maxCost.ToString() + ".");
                return false;
            }
            else if (hours < minTime || hours > maxTime)
            {
                System.Windows.Forms.MessageBox.Show("Hours should be between " + minTime.ToString() + " and " + maxTime.ToString() + ".");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
