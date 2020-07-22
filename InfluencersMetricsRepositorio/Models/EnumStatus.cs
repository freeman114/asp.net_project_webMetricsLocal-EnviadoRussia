
using MongoDB.Bson;
using System;

namespace webMetrics.Models
{
    public class EnumStatus
    {
        public static string SOLICITADO = "Solicitado";
        public static string PROCESSADO = "Processado";
        public static string DISPONIVEL = "Disponivel";
        public static string PENDENTE = "Pendente";
        public static string BLOQUEADO = "Bloqueado";
    }
}