﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecurionPay.Common;
using SecurionPay.Enums;
using SecurionPay.Exception;
using SecurionPay.Request;
using SecurionPayTests.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurionPayTests.Integration
{
    [TestClass]
    public class SubscriptionTests : IntegrationTest
    {
        CustomerRequestBuilder _customerRequestBuilder = new CustomerRequestBuilder();
        CardRequestBuilder _cardRequestBuilder = new CardRequestBuilder();
        ChargeRequestBuilder _chargeRequestBuilder = new ChargeRequestBuilder();
        PlanRequestBuilder _planRequestBuilder = new PlanRequestBuilder();

        [TestMethod]
        public async Task SubscribeWithNewCardTest()
        {
            try
            {
                var plan = await _gateway.CreatePlan(_planRequestBuilder.Build());

                var customerRequest = _customerRequestBuilder.Build();
                var customer = await _gateway.CreateCustomer(customerRequest);

                var cardRequest = _cardRequestBuilder.Build();
            
                var subscriptionRequest = new SubscriptionRequest() { CustomerId = customer.Id, PlanId = plan.Id,TrialEnd=DateTime.Now.AddDays(10), Card = cardRequest };
                var subscription = await _gateway.CreateSubscription(subscriptionRequest);

                customer = await _gateway.RetrieveCustomer(customer.Id);
                Assert.AreEqual(cardRequest.CardholderName, customer.Cards.First(c => c.Id == customer.DefaultCardId).CardholderName);

            }
            catch (SecurionPayException exc)
            {
                HandleApiException(exc);
            }
        }

        [TestMethod]
        public async Task SubscribeCaptureChargesByDefaultTest()
        {
            try
            {
                var plan = await _gateway.CreatePlan(_planRequestBuilder.Build());

                var customerRequest = _customerRequestBuilder.Build();
                var customer = await _gateway.CreateCustomer(customerRequest);

                var subscriptionRequest = new SubscriptionRequest() { CustomerId = customer.Id, PlanId = plan.Id, TrialEnd = DateTime.Now.AddDays(10) };
                var subscription = await _gateway.CreateSubscription(subscriptionRequest);

                customer = await _gateway.RetrieveCustomer(customer.Id);
                Assert.IsTrue(subscription.CaptureCharges);

            }
            catch (SecurionPayException exc)
            {
                HandleApiException(exc);
            }
        }

        [TestMethod]
        public async Task SubscribeWithAdressesTest()
        {
            var address = new AddressBuilder().Build();
            try
            {
                var plan = await _gateway.CreatePlan(_planRequestBuilder.Build());

                var customerRequest = _customerRequestBuilder.Build();
                var customer = await _gateway.CreateCustomer(customerRequest);

                var cardRequest = _cardRequestBuilder.WithCustomerId(customer.Id).Build();

                var subscriptionRequest = new SubscriptionRequest() { CustomerId = customer.Id, PlanId = plan.Id, Card = cardRequest, Billing = new Billing() { Address = address ,Name="name",Vat="123123"} };
                var subscription = await _gateway.CreateSubscription(subscriptionRequest);

                Assert.AreEqual(subscription.Billing.Address.FirstLine, address.FirstLine);
                Assert.AreEqual(subscription.Billing.Address.City, address.City);
                Assert.AreEqual(subscription.Billing.Address.CountryISOCode, address.CountryISOCode);
                Assert.AreEqual(subscription.Billing.Address.State, address.State);

            }
            catch (SecurionPayException exc)
            {
                HandleApiException(exc);
            }
        }

        [TestMethod]
        public async Task SubscribeWithCardFromChargeTest()
        {
            try
            {
                var plan = await _gateway.CreatePlan(_planRequestBuilder.Build());

                var customerRequest = _customerRequestBuilder.Build();
                var customer = await _gateway.CreateCustomer(customerRequest);

                var chargeRequest = _chargeRequestBuilder.WithCard(_cardRequestBuilder).Build();
                var charge = await _gateway.CreateCharge(chargeRequest);

                var subscriptionRequest = new SubscriptionRequest() { CustomerId = customer.Id, PlanId = plan.Id, Card = new CardRequest() { Id = charge.Id } };
                var subscription = await _gateway.CreateSubscription(subscriptionRequest);

                customer = await _gateway.RetrieveCustomer(customer.Id);
                Assert.AreEqual(chargeRequest.Card.CardholderName, customer.Cards.First(c => c.Id == customer.DefaultCardId).CardholderName);
            }
            catch (SecurionPayException exc)
            {
                HandleApiException(exc);
            }
        }
    }
}
