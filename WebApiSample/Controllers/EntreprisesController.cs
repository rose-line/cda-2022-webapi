using Contrats;
using Entites.DTO;
using Entites.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace WebApiSample.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class EntreprisesController : ControllerBase
  {
    private readonly IGestionRepos _gestionRepos;
    private readonly ILoggable _logger;

    public EntreprisesController(IGestionRepos gestionRepos, ILoggable logger)
    {
      _gestionRepos = gestionRepos;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult GetEntreprises()
    {
      try
      {
        var entreprises = _gestionRepos.Entreprises.GetEntreprises(tracked: false);
        var entreprisesDTOs = entreprises.Select(e => new EntrepriseDTO
        {
          Id = e.Id,
          Nom = e.Nom,
          AdresseComplete = string.Join(' ', e.Adresse, '-', e.Pays)
        }).ToList();

        return Ok(entreprisesDTOs);
      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès aux entreprises dans la méthode {nameof(GetEntreprises)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }
    }

    [HttpGet("{id}", Name = "EntrepriseParId")]
    public IActionResult GetEntreprise(Guid id)
    {
      try
      {
        var entreprise = _gestionRepos.Entreprises.GetEntreprise(id, tracked: false);
        if (entreprise == null)
        {
          _logger.LogInfo($"L'entreprise avec l'id: {id} n'existe pas en BDD.");
          return NotFound();
        }
        else
        {
          //var entrepriseDTO = _mapper.Map<EntrepriseDto>(entreprise);
          var entrepriseDTO = new EntrepriseDTO
          {
            Id = entreprise.Id,
            Nom = entreprise.Nom,
            AdresseComplete = string.Join(' ', entreprise.Adresse, '-', entreprise.Pays)
          };

          return Ok(entrepriseDTO);
        }
      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès à l'entreprise dans la méthode {nameof(GetEntreprise)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }
    }

    [HttpPost]
    public IActionResult CreerEntreprise([FromBody] EntrepriseCreationDTO entrepriseCreationDTO)
    {
      if (entrepriseCreationDTO == null)
      {
        _logger.LogErreur("EntrepriseCreationDTO reçu du client est null.");
        return BadRequest("Objet EntrepriseCreationDTO est null.");
      }

      var entreprise = new Entreprise
      {
        Nom = entrepriseCreationDTO.Nom,
        Adresse = entrepriseCreationDTO.Adresse,
        Pays = entrepriseCreationDTO.Pays,
      };
      //_mapper.Map<Company>(company);

      _gestionRepos.Entreprises.Creer(entreprise);
      _gestionRepos.Save();

      var entrepriseRetour = new EntrepriseDTO
      {
        Id = entreprise.Id,
        Nom = entreprise.Nom,
        AdresseComplete = string.Join(' ', entreprise.Adresse, '-', entreprise.Pays)
      };
      //_mapper.Map<CompanyDto>(companyEntity);

      return CreatedAtRoute("EntrepriseParId", new { id = entrepriseRetour.Id }, entrepriseRetour);
    }
  }

}
