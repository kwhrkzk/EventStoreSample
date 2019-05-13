
using System;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Utf8Json;

namespace Application
{
    public class ログイン情報DTO
    {
        public 利用者のID ID { get; set; }

        public long EventNumber { get; set; }

        public 氏名 氏名 { get; set; }

        public string ToJson() => System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(this)));
    }

    public interface Iログイン情報Query
    {
        ログイン情報DTO First();
    }
}
