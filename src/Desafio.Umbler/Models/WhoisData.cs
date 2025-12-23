using System;
using System.Collections.Generic;

namespace Desafio.Umbler.Models
{
    public class WhoisContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string PhoneExt { get; set; }
        public string Fax { get; set; }
        public string FaxExt { get; set; }
        public string Email { get; set; }
    }

    public class WhoisData
    {
        public string DomainName { get; set; }
        public string RegistryDomainId { get; set; }
        public string RegistrarWhoisServer { get; set; }
        public string RegistrarUrl { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Registrar { get; set; }
        public string RegistrarIanaId { get; set; }
        public List<string> DomainStatus { get; set; } = new List<string>();
        public string RegistryRegistrantId { get; set; }
        public WhoisContact Registrant { get; set; }
        public string RegistryAdminId { get; set; }
        public WhoisContact Admin { get; set; }
        public string RegistryTechId { get; set; }
        public WhoisContact Tech { get; set; }
        public string DnsSec { get; set; }
        public string RegistrarAbuseContactEmail { get; set; }
        public string RegistrarAbuseContactPhone { get; set; }
        public DateTime? LastUpdateOfWhoisDatabase { get; set; }
    }
}

