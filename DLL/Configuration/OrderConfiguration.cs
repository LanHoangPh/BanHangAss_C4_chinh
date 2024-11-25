using DLL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasMany<OrderDetail>(o => o.OrderDetails)
                .WithOne(od => od.Order).HasForeignKey(od => od.OrderId);

            builder.HasOne(o => o.PaymentHistory)
                .WithOne(ph => ph.Order)
                .HasForeignKey<PaymentHistory>(ph => ph.OrderId);
        }
    }
}
