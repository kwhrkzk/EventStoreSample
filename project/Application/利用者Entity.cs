
using System;
using System.Linq;
using System.Collections.Generic;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utf8Json;

namespace Application
{
    public class 利用者Entity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public long EventNumber { get; set; }

        [Required]
        public string 苗字 { get; set; }

        [Required]
        public string 名前 { get; set; }

        [Column("本一覧", TypeName = "json")]
        public List<Guid> 本一覧 { get; set; }
    }

    public class 利用者EntityConfiguration : IEntityTypeConfiguration<利用者Entity>
    {
        public void Configure(EntityTypeBuilder<利用者Entity> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.本一覧).HasConversion(
                v => System.Text.Encoding.UTF8.GetString(JsonSerializer.Serialize(v)),
                v => JsonSerializer.Deserialize<List<Guid>>(v));
        }
    }

    public static class 利用者EntityExtensions
    {
        public static void Copy(this 利用者Entity item, long _eventNumber, 利用者 _利用者)
        {
            item.EventNumber = _eventNumber;
            item.苗字 = _利用者.苗字文字列;
            item.名前 = _利用者.名前文字列;
            item.本一覧 = new List<Guid>(_利用者.貸出本一覧.Select(x => x.ID));
        }

        public static 利用者Entity Convert(this 利用者 _利用者, long _eventNumber)
        => new 利用者Entity
        {
            Id = _利用者.GUID,
            EventNumber = _eventNumber,
            苗字 = _利用者.苗字文字列,
            名前 = _利用者.名前文字列,
            本一覧 = _利用者.貸出本GUID一覧
        };
    }
}