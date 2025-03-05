using System;

using System.Collections.Generic;

using System.Linq;

using System.Xml.Linq;

using Microsoft.EntityFrameworkCore;


namespace WebApplication1.Models
{
    public class Shelter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Address Address { get; set; } // Referenční vlastnost pro adresu útulku
        // Další vlastnosti, jako např. telefonní číslo, obrázky atd.
    }
}

