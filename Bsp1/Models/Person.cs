using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bsp1.Models
{
    /// <summary>
    /// Personclass with 3 Properties (Name, Password, Hash)
    /// </summary>
    /// <value></value>
    public class Person
    {

        public string Name { get; set; }
        public string Password { get; set; }
        public string? Hash { get; set; }
    }
}