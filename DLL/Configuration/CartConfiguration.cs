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
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasMany<CartDetail>(c => c.CartDetails)
                .WithOne(cd => cd.Cart).HasForeignKey(cd => cd.CartDetailId);
                //.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
