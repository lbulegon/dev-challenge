using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Desafio.Umbler.Models;

namespace Desafio.Umbler.Helpers
{
    public static class WhoisParser
    {
        public static WhoisData Parse(string whoisRaw)
        {
            if (string.IsNullOrWhiteSpace(whoisRaw))
                return null;

            var data = new WhoisData();
            var lines = whoisRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            WhoisContact currentContact = null;
            string currentContactType = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Domain Name
                if (TryExtractValue(trimmedLine, "Domain Name:", out var domainName))
                {
                    data.DomainName = domainName;
                    continue;
                }

                // Registry Domain ID
                if (TryExtractValue(trimmedLine, "Registry Domain ID:", out var registryDomainId))
                {
                    data.RegistryDomainId = registryDomainId;
                    continue;
                }

                // Registrar WHOIS Server
                if (TryExtractValue(trimmedLine, "Registrar WHOIS Server:", out var registrarWhoisServer))
                {
                    data.RegistrarWhoisServer = registrarWhoisServer;
                    continue;
                }

                // Registrar URL
                if (TryExtractValue(trimmedLine, "Registrar URL:", out var registrarUrl))
                {
                    data.RegistrarUrl = registrarUrl;
                    continue;
                }

                // Updated Date
                if (TryExtractDate(trimmedLine, "Updated Date:", out var updatedDate))
                {
                    data.UpdatedDate = updatedDate;
                    continue;
                }

                // Creation Date
                if (TryExtractDate(trimmedLine, "Creation Date:", out var creationDate))
                {
                    data.CreationDate = creationDate;
                    continue;
                }

                // Expiration Date
                if (TryExtractDate(trimmedLine, "Registrar Registration Expiration Date:", out var expirationDate) ||
                    TryExtractDate(trimmedLine, "Registry Expiry Date:", out expirationDate))
                {
                    data.ExpirationDate = expirationDate;
                    continue;
                }

                // Registrar
                if (TryExtractValue(trimmedLine, "Registrar:", out var registrar))
                {
                    data.Registrar = registrar;
                    continue;
                }

                // Registrar IANA ID
                if (TryExtractValue(trimmedLine, "Registrar IANA ID:", out var registrarIanaId))
                {
                    data.RegistrarIanaId = registrarIanaId;
                    continue;
                }

                // Domain Status
                if (trimmedLine.StartsWith("Domain Status:", StringComparison.OrdinalIgnoreCase))
                {
                    var statusMatch = Regex.Match(trimmedLine, @"Domain Status:\s*(.+)", RegexOptions.IgnoreCase);
                    if (statusMatch.Success)
                    {
                        var status = statusMatch.Groups[1].Value.Trim();
                        if (!data.DomainStatus.Contains(status))
                            data.DomainStatus.Add(status);
                    }
                    continue;
                }

                // Registrar Abuse Contact Email
                if (TryExtractValue(trimmedLine, "Registrar Abuse Contact Email:", out var abuseEmail))
                {
                    data.RegistrarAbuseContactEmail = abuseEmail;
                    continue;
                }

                // Registrar Abuse Contact Phone
                if (TryExtractValue(trimmedLine, "Registrar Abuse Contact Phone:", out var abusePhone))
                {
                    data.RegistrarAbuseContactPhone = abusePhone;
                    continue;
                }

                // DNSSEC
                if (TryExtractValue(trimmedLine, "DNSSEC:", out var dnsSec))
                {
                    data.DnsSec = dnsSec;
                    continue;
                }

                // Last update of WHOIS database
                if (trimmedLine.Contains("Last update of WHOIS database:", StringComparison.OrdinalIgnoreCase))
                {
                    var dateMatch = Regex.Match(trimmedLine, @"Last update of WHOIS database:\s*(.+)", RegexOptions.IgnoreCase);
                    if (dateMatch.Success && TryParseWhoisDate(dateMatch.Groups[1].Value.Trim(), out var lastUpdate))
                    {
                        data.LastUpdateOfWhoisDatabase = lastUpdate;
                    }
                    continue;
                }

                // Registry Registrant ID - inicia seção Registrant
                if (TryExtractValue(trimmedLine, "Registry Registrant ID:", out var registryRegistrantId))
                {
                    data.RegistryRegistrantId = registryRegistrantId;
                    currentContactType = "Registrant";
                    if (data.Registrant == null)
                        data.Registrant = new WhoisContact();
                    currentContact = data.Registrant;
                    continue;
                }

                // Registry Admin ID - inicia seção Admin
                if (TryExtractValue(trimmedLine, "Registry Admin ID:", out var registryAdminId))
                {
                    data.RegistryAdminId = registryAdminId;
                    currentContactType = "Admin";
                    if (data.Admin == null)
                        data.Admin = new WhoisContact();
                    currentContact = data.Admin;
                    continue;
                }

                // Registry Tech ID - inicia seção Tech
                if (TryExtractValue(trimmedLine, "Registry Tech ID:", out var registryTechId))
                {
                    data.RegistryTechId = registryTechId;
                    currentContactType = "Tech";
                    if (data.Tech == null)
                        data.Tech = new WhoisContact();
                    currentContact = data.Tech;
                    continue;
                }

                // Contact sections - detectar início de uma nova seção de contato pelo nome
                if (trimmedLine.StartsWith("Registrant Name:", StringComparison.OrdinalIgnoreCase))
                {
                    currentContactType = "Registrant";
                    if (data.Registrant == null)
                        data.Registrant = new WhoisContact();
                    currentContact = data.Registrant;
                    // Continue para processar o nome na seção de campos de contato abaixo
                }
                else if (trimmedLine.StartsWith("Admin Name:", StringComparison.OrdinalIgnoreCase))
                {
                    currentContactType = "Admin";
                    if (data.Admin == null)
                        data.Admin = new WhoisContact();
                    currentContact = data.Admin;
                    // Continue para processar o nome na seção de campos de contato abaixo
                }
                else if (trimmedLine.StartsWith("Tech Name:", StringComparison.OrdinalIgnoreCase))
                {
                    currentContactType = "Tech";
                    if (data.Tech == null)
                        data.Tech = new WhoisContact();
                    currentContact = data.Tech;
                    // Continue para processar o nome na seção de campos de contato abaixo
                }

                // Contact fields (only process if we're in a contact section)
                if (currentContact != null)
                {
                    if (TryExtractValue(trimmedLine, $"{currentContactType} Name:", out var contactName))
                    {
                        currentContact.Name = contactName;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Organization:", out var org))
                    {
                        currentContact.Organization = org;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Street:", out var street))
                    {
                        currentContact.Street = street;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} City:", out var city))
                    {
                        currentContact.City = city;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} State/Province:", out var state) ||
                        TryExtractValue(trimmedLine, $"{currentContactType} State:", out state))
                    {
                        currentContact.State = state;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Postal Code:", out var postalCode))
                    {
                        currentContact.PostalCode = postalCode;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Country:", out var country))
                    {
                        currentContact.Country = country;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Phone:", out var phone))
                    {
                        currentContact.Phone = phone;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Phone Ext:", out var phoneExt))
                    {
                        currentContact.PhoneExt = phoneExt;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Fax:", out var fax))
                    {
                        currentContact.Fax = fax;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Fax Ext:", out var faxExt))
                    {
                        currentContact.FaxExt = faxExt;
                        continue;
                    }

                    if (TryExtractValue(trimmedLine, $"{currentContactType} Email:", out var email))
                    {
                        currentContact.Email = email;
                        continue;
                    }
                }
            }

            return data;
        }

        private static bool TryExtractValue(string line, string prefix, out string value)
        {
            value = null;
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = line.Substring(prefix.Length).Trim();
                return !string.IsNullOrWhiteSpace(value);
            }
            return false;
        }

        private static bool TryExtractDate(string line, string prefix, out DateTime? date)
        {
            date = null;
            if (TryExtractValue(line, prefix, out var dateString))
            {
                if (TryParseWhoisDate(dateString, out var parsedDate))
                {
                    date = parsedDate;
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseWhoisDate(string dateString, out DateTime date)
        {
            // Formatos comuns de data em WHOIS: "2024-11-26T00:05:03Z" ou "2024-11-26T00:05:03.000Z"
            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
                {
                    return true;
                }
            }

            // Tentar parse genérico
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
            {
                return true;
            }

            date = default;
            return false;
        }
    }
}

