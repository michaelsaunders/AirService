using System;
using System.Collections.Generic;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IOrderService
    {
        Order PlaceOrder(int venueId,
                         int customerId,
                         List<OrderItem> orderItems,
                         DateTime orderDate);

        //List<Order> GetCustomerOrders(int venueId, int customerId, int maxRecords);
        List<Order> GetCustomerOrderInCurrentSession(int venueId, int customerId);

        List<Order> GetCustomerCurrentOrders(int venueId,
                                             int iPadId,
                                             int customerId);

        Order GetCustomerOrder(int venueId, int customerId, int orderId);

        List<Order> GetOrdersForDevice(int venueId, int iPadId);

        List<Order> GetModifiedOrdersForDevice(int venueId, int iPadId, DateTime dateTimeSince);

        Order GetOrderForDevice(int venueId, int iPadId, int orderId);

        void UpdateOrderItemStatus(int venueId,
                                   int iPadId,
                                   int orderItemId,
                                   int newState,
                                   string messagem,
                                   int? serviceOption = null);

        void CancelOrder(int venueId, int orderId, string message);

        /// <summary>
        /// Fianlize all open orders for the customer and returns id of finalized session
        /// </summary>
        int FinializeCustomerOrders(int venueId, int customerId, string message);

        void RequestToFinalizeAllOrders(int venueId,
                                        int customerId);

        void UndoOrderItemStatus(int venueId,
                                 int iPadId,
                                 int orderItemId,
                                 string message);

        void ConfirmOrderItemPickup(int customerId, int orderItemId);

        OrderItem GetOrderItemForPrinting(int venueId, int ipadId, int orderItemId);
    }
}