﻿namespace OnlineShop.Store.Api.Options
{
    public class RabbitMQOptions
    {
        public const string SectionName = "RabbitMQ";

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EntityExchange { get; set; }
        public string EntityCreateQueue { get; set; }
        public string EntityUpdateQueue { get; set; }
        public string EntityDeleteQueue { get; set; }
        public string EmailExchange { get; set; }
        public string EmailSendQueue { get; set; }
    }
}
