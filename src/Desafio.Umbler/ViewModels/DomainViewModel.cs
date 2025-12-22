using System;
using System.Collections.Generic;
using Desafio.Umbler.Models;

namespace Desafio.Umbler.ViewModels
{
    public class DomainViewModel
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string HostedAt { get; set; }
        public List<string> NameServers { get; set; } = new List<string>();
        public DateTime? UpdatedAt { get; set; }
        public int? Ttl { get; set; }
        public int? Id { get; set; }
        public string WhoIs { get; set; }
        public WhoisData WhoisData { get; set; }
    }
}

