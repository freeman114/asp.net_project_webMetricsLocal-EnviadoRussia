using System;

namespace webMetrics.Models
{
    public class Usuario
    {
        public string UsuarioInstagram { get; set; }

        public string Senha { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime DataValidade { get; set; }

        public string Tipo { get; set; }

        public string Email { get; set; }

        public string TokenFacebook { get; set; }
        public string UserId { get; set; }

        public string AgenciaUserId { get; set; }
        public string NomeAgencia { get; set; }
        public string Cidade { get; set; }
        public string Outros { get; set; }
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Telefone { get; set; }
        public string Ddd { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public DateTime DataNascimento { get; set; }
        
        public string access_token_page { get; set; }
        public string name_page { get; set; }
        public string Key { get; internal set; }

        public string StatusCredito { get; set; }
        public DateTime DataUsoCredito { get; set; }

    }
}