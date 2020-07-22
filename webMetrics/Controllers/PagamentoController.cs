using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webMetrics.Models;
using Wirecard;
using Wirecard.Models;

namespace webMetrics.Controllers
{
    public class PagamentoController : Controller
    {
        private readonly Models.AppSettings _appSettings;
        private readonly IOptions<Models.AppSettings> _settings;

        public static Environments ambiente { get; set; }

        public PagamentoController(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
            _appSettings = appSettings.Value;
        }

        private async Task<Wirecard.WirecardClient> SetAmbiente(Wirecard.WirecardClient WC)
        {
            var token = "";
            var chave = "";
            ambiente = Environments.PRODUCTION;
            token = _appSettings.TokenMOIP;
            chave = _appSettings.ChaveMOIP;
#if DEBUG
            //ambiente = Environments.SANDBOX;
            //token = "I6TNGJK392BZNFOWJNM0BLPR9MDHUUTS";
            //chave = "JZCWSVK7JLTEWBP7ANLMI0TM2IPYNH4CYZQH7YZZ";
#endif
            WC = new Wirecard.WirecardClient(ambiente, token, chave);

            return WC;
        }

        // GET: Pagamento
        public async Task<ActionResult> Index(string tipo)
        {
            byte[] _out;
            if (!HttpContext.Session.TryGetValue("UsuarioFull_id", out _out))
            {
                return RedirectToAction("ComeceAqui", "Relatorios", new { msg = 99 });
            }

            var _id = HttpContext.Session.GetString("UsuarioFull_id");
            var UserId = HttpContext.Session.GetString("UserId");
            var repMongo = new Repository.MongoRep("", _settings, UserId);

            var usuario = await repMongo.ListarById<Models.Usuario>(new MongoDB.Bson.ObjectId(_id));
            var pagamentoPage = new Models.DTO.PagamentoPage()
            {
                Usuario = usuario.FirstOrDefault().Obj
            };


            string planoCode = "";

            if (tipo == Convert.ToInt32(EnumTipoPagamento.Influenciador).ToString())
            {//Influenciador
                pagamentoPage.Quantidade = 1;
                pagamentoPage.Valor = 99M;
                pagamentoPage.Total = 99M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan" + tipo.ToString() + "-99");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan" + tipo.ToString() + "-99",
                        "Plano-" + tipo.ToString() + "-Influencer", "Um influencer - 99 reais", 9900);

                }
                planoCode = "Plan" + tipo.ToString() + "-99";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas10).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 10;
                pagamentoPage.Valor = 99.9M;
                pagamentoPage.Total = 999M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan" + tipo.ToString() + "-999");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan" + tipo.ToString() + "-999",
                        "Plano-" + tipo.ToString() + "-Influencer", "10 influenciadores - 999 reais", 99900);

                }
                planoCode = "Plan" + tipo.ToString() + "-999";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas30).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 30;
                pagamentoPage.Valor = 93.3M;
                pagamentoPage.Total = 2799M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan" + tipo.ToString() + "-2799");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan" + tipo.ToString() + "-2799",
                        "Plano-" + tipo.ToString() + "-Influencer", "30 influenciadores - 2799 reais", 279900);

                }
                planoCode = "Plan" + tipo.ToString() + "-2799";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas90).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 90;
                pagamentoPage.Valor = 53.32M;
                pagamentoPage.Total = 4799M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan" + tipo.ToString() + "-4799");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan" + tipo.ToString() + "-4799",
                        "Plano-" + tipo.ToString() + "-Influencer", "90 influenciadores - 4799 reais", 479900);

                }
                planoCode = "Plan" + tipo.ToString() + "-4799";
            }

            if (tipo == Convert.ToInt32(EnumTipoPagamento.InfluenciadorYear).ToString())
            {//Influenciador
                pagamentoPage.Quantidade = 1;
                pagamentoPage.Valor = 999M;
                pagamentoPage.Total = 999M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan-yearY" + tipo.ToString() + "999");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan-yearY" + tipo.ToString() + "999",
                        "Plano-" + tipo.ToString() + "-Influencer", "Um influencer - 999 reais - Ano", 99900);
                    if (plano == null) RedirectToAction("login");
                }
                planoCode = "Plan-yearY" + tipo.ToString() + "999";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas180Year).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 180;
                pagamentoPage.Valor = 52.77M;
                pagamentoPage.Total = 9499M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan-Y" + tipo.ToString() + "9499");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan-Y" + tipo.ToString() + "9499",
                        "Plano-" + tipo.ToString() + "-Influencer", "180 influenciadores - 9499 reais", 949900);
                    if (plano == null) RedirectToAction("login");
                }
                planoCode = "Plan-Y" + tipo.ToString() + "9499";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas300Year).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 300;
                pagamentoPage.Valor = 49.33M;
                pagamentoPage.Total = 14799M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan-Y" + tipo.ToString() + "14799");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan-Y" + tipo.ToString() + "14799",
                        "Plano-" + tipo.ToString() + "-Influencer", "300 influenciadores - 14799 reais", 1479900);
                    if (plano == null) RedirectToAction("login");
                }
                planoCode = "Plan-Y" + tipo.ToString() + "14799";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.AgenciasMarcas600Year).ToString())
            {//Agencia
                pagamentoPage.Quantidade = 600;
                pagamentoPage.Valor = 49.165M;
                pagamentoPage.Total = 29499M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan-Y" + tipo.ToString() + "29499");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan-Y" + tipo.ToString() + "29499",
                        "Plano-" + tipo.ToString() + "-Influencer", "600 influenciadores - 29499 reais", 2949900);
                    if (plano == null) RedirectToAction("login");
                }
                planoCode = "Plan-Y" + tipo.ToString() + "29499";
            }
            if (tipo == Convert.ToInt32(EnumTipoPagamento.Testes).ToString())
            {//TESTES
                pagamentoPage.Quantidade = 1;
                pagamentoPage.Valor = 1.00M;
                pagamentoPage.Total = 1M;

                var planServer = await repMongo.ListarPlano<PlanResponse>("Plan" + tipo.ToString() + "-TST");
                if (planServer == null || (planServer.Count() == 0))
                {
                    var plano = await criarPlano("Plan" + tipo.ToString() + "-TST",
                        "Plano-" + tipo.ToString() + "-Influencer", "Um influencer - 1 real", 100);

                }
                planoCode = "Plan" + tipo.ToString() + "-TST";
            }

            ViewBag.hddPlan = planoCode;
            return View(pagamentoPage);
        }

        public async Task<PlanResponse> criarPlano(string code, string name, string description, int amount)
        {
            Wirecard.WirecardClient WC = null;
            WC = await SetAmbiente(WC);
            try
            {
                var newPlan = new PlanRequest()
                {
                    Code = code,
                    Name = name,
                    Description = description,
                    Amount = amount,
                    Interval = new Interval()
                    {
                        Unit = (code.Contains("year") ? "YEAR":"MONTH"),
                        Length = 1
                    },
                    Payment_Method = "CREDIT_CARD"
                };

                var plan = await WC.Signature.CreatePlan(newPlan);

                var planNew = new PlanResponse()
                {
                    Code = code,
                    Name = name,
                    Description = description,
                    Amount = amount,
                    Payment_Method = "CREDIT_CARD"
                };
                var repMongo = new Repository.MongoRep("", _settings, "");
                await repMongo.GravarOne<PlanResponse>(planNew);

                return plan;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<SubscriptionResponse> CreateSignature(Address address, Credit_Card creditCard, string cpf, DateTime birth,
            string email, string fullname, string phone_area_code, string phone_number, string codePlan, string id, string clientId)
        {
            try
            {
                Wirecard.WirecardClient WC = null;
                WC = await SetAmbiente(WC);

                var birthdate_day = birth.Day.ToString();
                var birthdate_month = birth.Month.ToString();
                var birthdate_year = birth.Year.ToString();

                var subscriber = await WC.Signature.CreateSubscriptions(new SubscriptionRequest()
                {
                    Plan = new Plan()
                    {
                        Name = codePlan,
                        Code = codePlan
                    },
                    Code = id,
                    Payment_Method = "CREDIT_CARD",
                    Customer = new Customer()
                    {
                        Email = email,
                        Phone_Number = phone_number,
                        Phone_Area_Code = phone_area_code,
                        FullName = creditCard.Holder_Name,
                        Address = address,
                        BirthDate = birth.ToString("dd/MM/YYYY"),
                        Phone = new Phone()
                        {
                            AreaCode = phone_area_code,
                            CountryCode = "55",
                            Number = phone_number
                        },
                        Billing_Info = new Billing_Info()
                        {
                            Credit_Card = creditCard
                        },
                        Code = clientId,
                        Cpf = cpf,
                        BirthDate_Day = Convert.ToInt32(birthdate_day),
                        BirthDate_Month = birthdate_month,
                        BirthDate_Year = Convert.ToInt32(birthdate_year)
                    }
                }, true);

                return subscriber;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<ActionResult> Pagamento(Models.DTO.PagamentoPage pagamentoPage)
        {
            var _id = HttpContext.Session.GetString("UsuarioFull_id");
            var UserId = HttpContext.Session.GetString("UserId");
            var repMongo = new Repository.MongoRep("", _settings, UserId);

            var usuario = await repMongo.ListarById<Models.Usuario>(new MongoDB.Bson.ObjectId(_id));
            pagamentoPage.Usuario.UserId = UserId;

            var result = await CreatePay(pagamentoPage, _id.ToString() + "-" + DateTime.Now.ToLongTimeString().Replace(":", "-"), UserId, repMongo, pagamentoPage.codPlan);
            result.StatusPagamento = "Pendente";

            await repMongo.GravarOne<Models.DTO.PagamentoPage>(result);
            if (usuario.FirstOrDefault().Obj.Tipo == "1")
            {
                return RedirectToAction("MinhasAnalises", "Relatorios");
            }
            else
            {
                return RedirectToAction("HistoricoMetricas", "Relatorios");
            }
        }

        private async Task<Models.DTO.PagamentoPage> CreatePay(Models.DTO.PagamentoPage pay, string _id, string UserId, Repository.MongoRep mongoRep, string plan)
        {
            Wirecard.WirecardClient WC = null;
            WC = await SetAmbiente(WC);

            var OrderId = Guid.NewGuid().ToString();
            try
            {
                pay.OrderId = OrderId;
                pay.Usuario.Telefone = Helper.ApenasNumeros(pay.Usuario.Telefone);

                var usuario = pay.Usuario;

                var sub = await CreateSignature(new Address()
                {
                    City = usuario.City,
                    Complement = usuario.Complement,
                    District = usuario.District,
                    Street = usuario.Street,
                    StreetNumber = usuario.Number,
                    ZipCode = usuario.PostalCode.Replace("-", ""),
                    State = usuario.State,
                    Country = "BRA"
                }, new Credit_Card
                {
                    Holder_Name = pay.Usuario.Nome,
                    Number = pay.cardNumber,
                    Expiration_Year = pay.expirationYear,
                    Expiration_Month = pay.expirationMonth
                }, usuario.Cpf, usuario.DataNascimento, usuario.Email, usuario.Nome + " " + usuario.Sobrenome, usuario.Ddd, usuario.Telefone, plan, OrderId, _id);

                //var cliente = await CriarClienteAsync(pay.Usuario, OrderId, WC);
                //var pedido = await CriarPedidoAsync(pay, cliente.Id, WC);
                //var pagamento = await CriarPagamentoAsync(cliente, pedido, pay, WC);

                //pay.customerResponse = cliente;
                //pay.orderResponse = pedido;
                //pay.paymentResponse = pagamento;

                if (sub !=null && ((sub.Alerts!= null) &&(sub.Alerts.Count()>0)))
                {
                    var resultAlerts = JsonConvert.SerializeObject(sub.Alerts).ToString();
                    var repMongo = new Repository.MongoRep("", _settings, "");
                    await repMongo.GravarOne<string>("Pagamento: {" + sub.Code + "}" + resultAlerts);

                    sub.Alerts = null;
                }

                pay.subscriptionResponse = sub;
                return pay;
            }
            catch (Exception ex)
            {
                var text = ex.Message.ToString();
                return null;
            }
        }

        protected async Task<CustomerResponse> CriarClienteAsync(Usuario usuario, string _id, Wirecard.WirecardClient WC)
        {
            var body = new CustomerRequest
            {
                OwnId = _id,
                FullName = usuario.Nome,
                Email = usuario.Email,
                BirthDate = usuario.DataNascimento.ToString("yyyy-MM-dd"),
                TaxDocument = new Taxdocument
                {
                    Type = "CPF",
                    Number = usuario.Cpf
                },
                Phone = new Phone
                {
                    CountryCode = "55",
                    AreaCode = usuario.Ddd,
                    Number = usuario.Telefone
                },
                ShippingAddress = new Shippingaddress
                {
                    City = usuario.City,
                    Complement = usuario.Complement,
                    District = usuario.District,
                    Street = usuario.Street,
                    StreetNumber = usuario.Number,
                    ZipCode = usuario.PostalCode.Replace("-", ""),
                    State = usuario.State,
                    Country = "BRA"
                }
            };
            var result = await WC.Customer.Create(body);
            return result;
        }

        protected async Task<OrderResponse> CriarPedidoAsync(Models.DTO.PagamentoPage pagamento, string CustomerId, Wirecard.WirecardClient WC)
        {
            var body = new OrderRequest
            {
                OwnId = pagamento.OrderId,
                Amount = new Amount
                {
                    Currency = "BRL",
                    Subtotals = new Subtotals
                    {

                        Shipping = 0// Convert.ToInt32(pagamento.Total * 100)
                    }
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Product = "Para influenciadores, marcas e agências",
                        Category = "ARTS_AND_ENTERTAINMENT",
                        Quantity = pagamento.Quantidade,
                        Detail = "",
                        Price = Convert.ToInt32(pagamento.Valor*100)
                    }
                },
                Customer = new Customer
                {
                    Id = CustomerId
                }
            };
            var result = await WC.Order.Create(body);
            return result;
        }

        [HttpGet]
        public async Task<bool> GetPayment(string pagamentoId)
        {
            try
            {
                Wirecard.WirecardClient WC = null;
                WC = await SetAmbiente(WC);

                var _id = new ObjectId(pagamentoId);
                var repMongo = new Repository.MongoRep("", _settings, "");

                var lstPagamentos = await repMongo.ListarById<Models.DTO.PagamentoPage>(_id);
                if (lstPagamentos != null)
                {
                    var userId = lstPagamentos.FirstOrDefault().UsuarioId;
                    var _pagamentoAtual = lstPagamentos.FirstOrDefault().Obj;
                    var _pagamentoAtualContractual = lstPagamentos.FirstOrDefault();
                    if (_pagamentoAtual.paymentResponse != null)
                    {
                        #region Pagamentos Comuns
                        var result = await WC.Payment.Consult(_pagamentoAtual.paymentResponse.Id);

                        if (result.Status != _pagamentoAtual.paymentResponse.Status)
                        {
                            _pagamentoAtual.paymentResponse.Status = result.Status;
                            if (_pagamentoAtual.StatusPagamento == "Pendente" && result.Status == "AUTHORIZED")//Pago
                            {
                                _pagamentoAtual.StatusPagamento = "Pago";
                            }

                            //Mudar Status
                            await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                            {
                                _id = _id,
                                Obj = _pagamentoAtual
                            });

                            if (_pagamentoAtual.StatusPagamento == "Pago")
                            {
                                //Inserir credito se for authorizado o pagamento
                                var credito = new Models.CreditoMetricas()
                                {
                                    UserId = userId,
                                    Qtd = _pagamentoAtual.Quantidade,
                                    DataCredito = DateTime.Now,
                                    Debito = 0,
                                    DataValidade = DateTime.Now.AddMonths(1),
                                    DataCriacao = DateTime.Now
                                };
                                await repMongo.GravarOne<Models.CreditoMetricas>(credito);
                            }
                        }
                        if (result.Status == "CANCELLED" && _pagamentoAtual.StatusPagamento == "Pendente")
                        {
                            _pagamentoAtual.StatusPagamento = "Cancelado";
                            //Mudar Status
                            await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                            {
                                _id = _id,
                                Obj = _pagamentoAtual
                            });
                        }
                        if (result.Status == "REFUNDED" && _pagamentoAtual.StatusPagamento == "Pendente")
                        {
                            _pagamentoAtual.StatusPagamento = "Cancelado";
                            //Mudar Status
                            await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                            {
                                _id = _id,
                                Obj = _pagamentoAtual
                            });
                        }
                        if (result.Status == "REFUNDED" && _pagamentoAtual.StatusPagamento == "Pago")
                        {
                            _pagamentoAtual.StatusPagamento = "Cancelado";
                            //Mudar Status
                            await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                            {
                                _id = _id,
                                Obj = _pagamentoAtual
                            });
                        }
                        #endregion
                    }
                    else
                    {
                        if (_pagamentoAtual.subscriptionResponse != null)
                        {
                            #region Invoices
                            var lstResult = await WC.Signature.ListSignatureInvoices(_pagamentoAtual.subscriptionResponse.Code);
                            foreach (var result in lstResult.Invoices)
                            {
                                if (_pagamentoAtual.Invoices == null)
                                {
                                    _pagamentoAtual.Invoices = new List<Invoice>();
                                }
                                var _invoice = _pagamentoAtual.Invoices.Where(w => w.Id == result.Id).FirstOrDefault();

                                if (_invoice == null)
                                {
                                    _pagamentoAtual.Invoices.Add(result);
                                    _invoice = result;
                                }

                                if (result.Status.Code == 3)//Pago
                                {
                                    _pagamentoAtual.StatusPagamento = "Pago";
                                    _pagamentoAtual.NextInvoice = DateTime.Now.AddMonths(1);

                                    //Mudar Status
                                    await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                                    {
                                        _id = _id,
                                        Obj = _pagamentoAtual
                                    });

                                    //Inserir credito se for authorizado o pagamento
                                    var credito = new Models.CreditoMetricas()
                                    {
                                        UserId = userId,
                                        Qtd =  _pagamentoAtual.Quantidade,
                                        DataCredito = DateTime.Now,
                                        Debito = 0,
                                        DataValidade =
                                        _pagamentoAtual.codPlan.Contains("year")? 
                                            DateTime.Now.AddMonths(12) :DateTime.Now.AddMonths(1),
                                            DataCriacao = DateTime.Now
                                    };
                                    await repMongo.GravarOne<Models.CreditoMetricas>(credito);

                                    //Email de pagamento
                                    var usuarioId = await repMongo.FindFilter<Models.Usuario>("Obj.UserId", _pagamentoAtual.Usuario.UserId);
                                    var envio = SenderEmail.Pagamento(_pagamentoAtual.Usuario.Email, usuarioId._id.ToString() );
                                }

                                if (_invoice.Status != result.Status)
                                {
                                    if (result.Status.Code == 4)//Problemas no pagto
                                    {
                                        _pagamentoAtual.StatusPagamento = "Problemas";

                                        //Mudar Status
                                        await repMongo.AlterarStatusPagamento(new ContractClass<Models.DTO.PagamentoPage>()
                                        {
                                            _id = _id,
                                            Obj = _pagamentoAtual
                                        });

                                    }
                                }
                            }

                            _pagamentoAtualContractual.Obj = _pagamentoAtual;
                            await repMongo.AlterarInvoices(_pagamentoAtualContractual);

                            #endregion
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected async Task<PaymentResponse> CriarPagamentoAsync(CustomerResponse cliente, OrderResponse pedido, Models.DTO.PagamentoPage pagamento, Wirecard.WirecardClient WC)
        {
            var body = new PaymentRequest
            {
                //informe os campos aqui
                InstallmentCount = 1,
                FundingInstrument = new Fundinginstrument
                {
                    Method = "CREDIT_CARD",
                    CreditCard = new Creditcard
                    {
                        Hash = pagamento.chave,
                        Holder = new Holder
                        {
                            FullName = pagamento.Usuario.Nome,
                            BirthDate = pagamento.Usuario.DataNascimento.ToString("yyyy-MM-dd"),
                            TaxDocument = new Taxdocument
                            {
                                Type = "CPF",
                                Number = pagamento.Usuario.Cpf
                            }
                        }
                    }
                }
            };
            var result = await WC.Payment.Create(body, pedido.Id);
            return result;
        }
    }
}