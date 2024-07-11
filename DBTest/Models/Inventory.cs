namespace DBTest.models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Table("Inventory")]
internal class Inventory
{
    [Key]
    [Column("id")]
    public int Id;

    [Column("name")]
    public string Name;

    [Column("quantity")]
    public int Quantity;

    public override string ToString()
    {
        return $"""
            Inventory[Id={Id}, Name={Name},Quantity={Quantity}]
            """;
    }
}
