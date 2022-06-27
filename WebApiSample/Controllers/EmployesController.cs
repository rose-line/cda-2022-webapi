using Contrats;
using Entites.DTO;
using Entites.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace WebApiSample.Controllers
{
  [Route("api/entreprises/{entrepriseId}/[controller]")]
  [ApiController]
  public class EmployesController : ControllerBase
  {
    private readonly IGestionRepos _gestionRepos;
    private readonly ILoggable _logger;

    public EmployesController(IGestionRepos gestionRepos, ILoggable logger)
    {
      _gestionRepos = gestionRepos;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult GetEmployesDeLEntreprise(Guid entrepriseId)
    {
      try
      {
        var entreprise = _gestionRepos.Entreprises.GetEntreprise(entrepriseId, tracked: false);
        if (entreprise == null)
        {
          _logger.LogInfo($"L'entreprise avec l'id: {entrepriseId} n'existe pas en BDD.");
          return NotFound();
        }
      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès à l'entreprise dans la méthode {nameof(GetEmployesDeLEntreprise)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }

      try
      {
        var employes = _gestionRepos.Employes.GetEmployes(entrepriseId, tracked: false);

        var employesDTOs = employes.Select(e => new EmployeDTO
        {
          Id = e.Id,
          Nom = e.Nom,
          Age = e.Age,
          Poste = e.Poste
        }).ToList();

        return Ok(employesDTOs);
      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès aux employés dans la méthode {nameof(GetEmployesDeLEntreprise)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }
    }

    [HttpGet("{employeId}", Name = "EmployeDeEntreprise")]
    public IActionResult GetEmployeDeLEntreprise(Guid entrepriseId, Guid employeId)
    {
      try
      {
        var entreprise = _gestionRepos.Entreprises.GetEntreprise(entrepriseId, tracked: false);

        if (entreprise == null)
        {
          _logger.LogInfo($"L'entreprise avec l'id: {entrepriseId} n'existe pas en BDD.");
          return NotFound();
        }
      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès à l'entreprise dans la méthode {nameof(GetEmployeDeLEntreprise)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }

      try
      {
        var employe = _gestionRepos.Employes.GetEmployeDeLEntreprise(entrepriseId, employeId, tracked: false);

        if (employe == null)
        {
          _logger.LogInfo($"L'employe avec l'id: {employeId} n'existe pas en BDD.");
          return NotFound();
        }

        var employeDTO = new EmployeDTO
        {
          Id = employe.Id,
          Nom = employe.Nom,
          Age = employe.Age,
          Poste = employe.Poste
        };

        return Ok(employeDTO);

      }
      catch (Exception ex)
      {
        _logger.LogErreur($"Problème dans l'accès à l'employé dans la méthode {nameof(GetEmployeDeLEntreprise)} {ex}");
        return StatusCode(500, "Erreur Serveur");
      }
    }

    [HttpPost]
    public IActionResult CreerPourEntreprise(Guid entrepriseId, [FromBody] EmployeCreationDTO employeCreationDTO)
    {
      if (employeCreationDTO == null)
      {
        _logger.LogErreur("EmployeCreationDTO reçu du client est null.");
        return BadRequest("Objet EmployeCreationDTO est null.");
      }

      var entreprise = _gestionRepos.Entreprises.GetEntreprise(entrepriseId, tracked: false);
      if (entreprise == null)
      {
        _logger.LogInfo($"L'entreprise avec l'id: {entrepriseId} n'existe pas en BDD.");
        return NotFound();
      }

      var employe = new Employe
      {
        Nom = employeCreationDTO.Nom,
        Age = employeCreationDTO.Age,
        Poste = employeCreationDTO.Poste
      };
      //_mapper.Map<Employee>(employee);

      _gestionRepos.Employes.CreerPourEntreprise(entrepriseId, employe);
      _gestionRepos.Save();

      var employeRetour = new EmployeDTO
      {
        Id = employe.Id,
        Nom = employe.Nom,
        Age = employe.Age,
        Poste = employe.Poste
      };
      //_mapper.Map<EmployeeDto>(employeeEntity);

      return CreatedAtRoute(
        "EmployeDeEntreprise",
        new { entrepriseId = entrepriseId, employeId = employeRetour.Id },
        employeRetour);
    }
  }
}
