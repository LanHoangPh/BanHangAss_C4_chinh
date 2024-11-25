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
    public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
    {
        public void Configure(EntityTypeBuilder<PaymentHistory> builder)
        {
            builder.HasOne<PaymentMethod>(ph => ph.PaymentMethod)
                .WithMany(pm => pm.PaymentHistories)
                .HasForeignKey(ph => ph.PaymentMethodId);
        }
    }
}
