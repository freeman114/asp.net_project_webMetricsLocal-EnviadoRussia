using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webMetrics.Models.DTO;
using webMetrics.Repository;

namespace webMetrics.Controllers
{
    public class PainelController : Controller
    {
        public async  Task<IActionResult> Index()
        {
            List<DadosDescricao> dados = new List<DadosDescricao>();

            MongoSearchRep rep = new MongoSearchRep();
            var a = await rep.GetValue("SearchInfluencers.Domain.Entities.InfluencerEntity");
            dados.Add(new DadosDescricao() { Description = "Total de influencers disponíveis na busca", Valor = a.Elements.ToList()[0].Value.ToInt32().ToString("N0") });

            dados.Add(new DadosDescricao() { Description = "----------------------------------------------", Valor = "--" });

            var b = await rep.GetValue("Infs.Model.Influencers");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Instagram disponíveis na base", Valor = b.Elements.ToList()[0].Value.ToInt32().ToString("N0") });
            var b1 = await rep.GetCountEqual("Infs.Model.Influencers", "Type", "5");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Instagram com menos de 12800 seguidores disponíveis na base", Valor = b1.ToString("N0") });

            var c = await rep.GetValue("Infs.Model.TwitterInfluencers");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Twitter disponíveis na base", Valor = c.Elements.ToList()[0].Value.ToInt32().ToString("N0") });
            var c1 = await rep.GetCountEqual("Infs.Model.TwitterInfluencers", "Type", "5");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Twitter com menos de 12800 seguidores disponíveis na base", Valor = c1.ToString("N0") });

            var d = await rep.GetValue("Infs.Model.YoutubeInfluencers");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Youtube disponíveis na base", Valor = d.Elements.ToList()[0].Value.ToInt32().ToString("N0") });
            var d1 = await rep.GetCountEqual("Infs.Model.YoutubeInfluencers", "Type", "5");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Youtube com menos de 12800 seguidores disponíveis na base", Valor = d1.ToString("N0") });

            dados.Add(new DadosDescricao() { Description = "----------------------------------------------", Valor = "--" });

            var e = await rep.GetCountEqual("SearchInfluencers.Domain.Entities.InfluencerEntity", "Origin", "I");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Instagram disponíveis na busca", Valor = e.ToString("N0") });
            var f = await rep.GetCountEqual("SearchInfluencers.Domain.Entities.InfluencerEntity", "Origin", "T");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Twitter disponíveis na busca", Valor = f.ToString("N0") });
            var g = await rep.GetCountEqual("SearchInfluencers.Domain.Entities.InfluencerEntity", "Origin", "Y");
            dados.Add(new DadosDescricao() { Description = "Total de usuários do Youtube disponíveis na busca", Valor = g.ToString("N0") });

            //var e = await rep.ExecuteMongoDBCommand("SearchInfluencers.Domain.Entities.InfluencerEntity");
            //dados.Add(new DadosDescricao() { Description = "Total de usuários do Youtube disponíveis na base", Valor = e.ToString() });


            return View(dados);
        }

    }
}