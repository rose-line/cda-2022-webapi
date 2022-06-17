# API ASP.NET

## Environnement

Visual Studio 2019 ou 2022 (le code présenté target .NET 5 pour compatibilité avec VS2019)

## Création projet

- ASP.NET Core Web API
- HTTPS, OpenAPI

## Exploration

### `launchSettings`

- Configuration générale de l'application : `launchSettings.json` (dans `Properties`)
- Contient par défaut configurations pour IIS et pour l'auto-hébergement multi-plateformes (Kestrel)
- `launchBrowser` à `false`, si on souhaite éviter de lancer le navigateur sur l'url pointée par `launchUrl` sur le serveur indiqué
  - ça donnerait `https://localhost:5001/weatherforecast` en HTTPS avec la config par défaut, si on a coché HTTPS auparavant
  - mais on fait une API Web, pas un site, donc on va mettre `launchBrowser` à `false`
  - `launchUrl` n'a de sens que si `launchBrowser` est à `true`
- Attention, la configuration HTTPS est seulement valide dans cet environement local ; il faudra configurer un certificat valide et la redirection HTTPS au déploiement

### `Program.cs`

- Point d'entrée de l'application
- Par défaut, `Host.CreateDefaultBuilder` va utiliser Kestrel pour le _hosting_
- `webBuilder.UseStartup<Startup>` indique la classe d'initialisation (`Startup`), qui va configurer les services

### `Startup.cs`

- Deux méthodes importantes :
  - `ConfigureServices` : les services !
    - `AddControllers` va enregistrer les controllers de `IServiceCollection`
    - on ne s'occupe pas des Views (Web API)
  - `Configure` : pour le _pipeline_ HTTP, c'est-à-dire qu'est-ce qui se passe pour chaque requête, ajout de _middlewares_ pour routage, configuration IIS, sécurité, etc... `UseEndpoints` va associer les endpoints au routing voulu (pour l'instant sans spécifier aucune route)
  - Attention, l'ordre dans le _pipeline_ est important, par exemple :
    - si on utilise CORS (`UseCors()`,prévention attaque _Cross Domain_), il doit être configuré _avant_ le routage
    - si on utilise des fichiers statiques (`UseStaticFiles()`, par défaut servis depuis le répertoire `wwwroot` du projet), ça doit être configuré _avant_ le routage

## Variables d'environnement

- Par défaut, environnement « Development »
- Variable `ASPNETCORE_ENVIRONMENT` dans `lauchSettings.json`
- En production, passez en « Production »
- Les environnements de dev et de prod doivent avoir différents URL, ports, connection strings, mots de passe, etc.
- `appSettings.json` est le fichier de configuration principal
- Tout ce qui est dans `appSettings.Development.json` (VS le montre « relié » au précédent) va écraser toutes les paires clé/valeurs éventuellement redéfinies (et en créer de nouvelles si nécessaire)
- Pour la prod, on va ajouter un `appSettings.Production.json`
- Pour indiquer dans quel environnement on est sur le système, on peut poser la variable d'environnement `ASPNETCORE_ENVIRONMENT` directement
- Sous Windows : `set ASPNETCORE_ENVIRONMENT=Production`
- Sous Linux : `export ASPNETCORE_ENVIRONMENT=Production`
- Le fichier `appSettings.Production.json` sera alors pris en compte
- Dans l'environnement de dev, `appSettings.json` pose ça pour nous en « Development »

# Configuration d'un service de Logging

## NLog

- .Net Core fournit son propre service de Logging
- MS change souvent d'avis et/ou d'API pour ce genre de choses
- On préfèrera utiliser une bibliothèque indépendante et éprouvée : **NLog**
- **Le logging est essentiel pour les applications en production**
- J'ai mis en gras mais ça ne sert à rien, vous n'en ferez quand même pas, ça saoule
- Jusqu'à ce que vous passiez deux semaines à debugger un problème en prod que vous ne parvenez pas à reproduire / identifier
- Et par la suite vous mordrez le premier collègue qui oubliera de logger ses événements / erreurs

## Mise en place projets

- Nouveau projet : `Contrats`, pour nos interfaces (_.Net Core Class Library_)
- Nouveau projet : `ServiceLogging`, pour notre service de logging
- `ServiceLogging` doit référencer `Contrats`
- Le projet principal doit référencer `ServiceLogging` (et `Contrats` par transitivité)

## Interface `ILoggable`

- Dans le projet `Contrats` (les interfaces sont des « contrats »)
- Première interface, elle va simplement retranscrire ce que fait un logger, et notamment celui de *NLog* :

```cs
public interface ILoggable
{
	void LogInfo(string message);

	void LogAvertissement(string message);

	void LogDebug(string message);

	void LogErreur(string message);
}
```

## Classe `Logger`

- Dans projet `ServiceLogging`
- D'abord, on installe _NLog_ via NuGet
  - Attention, on installe pas tout _NLog_, seulement la bib : `NLog.Extensions.Logging`
- Classe `Logger` va implémenter `ILoggable` et utiliser le logger de *NLog* :

```cs
public class Logger : ILoggable
{
	private static ILogger logger = LogManager.GetCurrentClassLogger();

	public void LogDebug(string message)
	{
		logger.Debug(message);
	}

	public void LogErreur(string message)
	{
		logger.Error(message);
	}

	public void LogInfo(string message)
	{
		logger.Info(message);
	}

	public void LogAvertissement(string message)
	{
		logger.Warn(message);
	}
}
```

- C'est donc juste un _wrapper_ (une enveloppe) autour de _NLog_
- Et à quoi ça sert, du coup ? Hein ? Franchement ?
- Eh bien si on veut changer de bib de logging plus tard, on n'aura que cette classe à modifier
- Ça s'appelle le _pattern Adapter_ et c'est très utile

## Configuration _NLog_

- _NLog_ doit savoir où seront localisés les fichiers de logs sur le système, leurs noms, et le niveau de logging souhaité
- Fichier `nlog.config` à la racine du projet :

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Trace"
      internalLogFile="C:\chemin_vers_les_logs_de_l_app\internals\internal_log.txt">

  <targets>
    <target name="logfile" xsi:type="File"
    fileName="C:\chemin_vers_les_logs_de_l_app\logs\${shortdate}_log.txt"
    layout="${longdate} ${level:uppercase=true} ${message}"/>
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="logfile" />
  </rules>

</nlog>
```

- `internalLogFile` désigne le fichier de logging interne de _NLog_
- `fileName` désigne le fichier de logging de l'application
- En réalité il peut y en avoir plusieurs : ici `${shortdate}` va en créer un par jour automatiquement
- Le `layout` désigne la façon dont les lignes de logs sont rendues et les infos associées
- Ici on aura par exemple un fichier `2021-06-04_log.txt`

## Service de logging

- Dans `Startup`, on va charger ce fichier de configuration dans le constructeur :

```cs
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
```

- Ensuite on va ajouter notre service de logging au container IoC (_Inversion of Control_) de .NET Core

## IoC et injection de dépendances

- Un container IoC est une classe (ou un ensemble de classes) qui gère et fournit les dépendances dont les autres closses ont besoin
- Les dépendances sont injectées par le constructeur : lorsqu'un constructeur déclare avoir besoin d'un certain type d'objet (ex. un logger), le container IoC va instancier la dépendance et l'injecter automatiquement
- Cela permet de découpler les objets de leur dépendances
- Un container IoC est donc une sorte de fabrique d'objets transversale à toute l'application
- Cela n'a de réel intérêt que si l'on utilise des interfaces : les classes réclame une dépendance sous forme d'interface, et le container lui donne une instance de classe concrète qui implémente cette interface
- Par exemple, notre IoC va être configuré ainsi : si une classe réclame un `ILoggable`, il va injecter une instance de `Logger`, qui se trouve, en ce qui nous concerne, utiliser _NLog_

## Enregistrement du service de logging

- On peut enregistrer notre service de logging de plusieurs manières dans le container IoC :
  - la méthode `AddSingleton` utilise le *pattern Singleton* : **un seul service** est créé, tous ceux qui en ont besoin récupèrent la même instance
  - avec `AddScoped` => un service est créé **par requête** HTTP
  - avec `AddTranscient` => un service est créé **chaque fois** qu'un composant en a besoin, même plusieurs fois par requête s'il le faut
- Enregistrons notre service de logging en requérant un logger par requête, avec `AddScoped`
- On doit appeler cette méthode via l'objet de type `IServiceCollection` de `ConfigureServices` : `services.AddScoped<ILoggable, Logger>()`

## Méthodes d'extension

- Pour éviter d'encombrer la méthode `ConfigureServices` avec de potentiels nombreux services et de nombreuses dépendances, on va utiliser une fonctionnalité de C# : les **méthodes d'extension**, qui vont nous permettre d'ajouter des méthodes à une classe dans des fichiers séparés sans même avoir accès au code de ladite classe
- Les méthodes d'extension sont statiques et doivent être dans une classe statique, que l'on va placer dans un répertoire `Extensions` :

```cs
public static class ServiceExtensions
{
	public static void ConfigureServiceLogging(this IServiceCollection services) => services.AddScoped<ILoggable, Logger>();
}
```

- Le `AddScoped` prend deux types : ce qui est demandé (la dépendance) et ce qui sera injecté
- Donc quand on demandera un `ILoggable`, on aura un objet de type `Logger`
- Notez le mot-clé `this` sur le paramètre : cela définit une méthode d'extension sur la classe ou interface du type indiqué (ici `IServiceCollection`)
- Dorénavant, tous les objets de ce type disposent de la méthode `ConfigureServiceLogging`
- On va remplacer notre appel « en dur » dans `Configure` par cette nouvelle méthode :

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.ConfigureServiceLogging();

	services.AddControllers();
}
```

## Injection du service

- Mettons en pratique dans le controller-exemple du projet
- On remplace le logger par défaut de .NET Core par notre `ILoggable`
- C'est ce type qui va être injecté dans le constructeur :

```cs
public WeatherForecastController(ILoggable logger)
{
	_logger = logger;
}
```

- Le container IoC va faire son boulot et injecter un objet `Logger`
- C'est pour ça que ça s'appelle « inversion du contrôle » : les dépendances ne sont pas contrôlées par l'objet qui en a directement besoin, mais par une entité extérieure
- Ajoutons quelques lignes à la méthode `Get()` pour tester le logger :

```cs
_logger.LogInfo("Message Info");
_logger.LogDebug("Message Debug");
_logger.LogAvertissement("Message Avertissement");
_logger.LogErreur("Message Erreur");
```

- Tapons ce _endpoint_ via, par exemple, le navigateur : `https://localhost:5001/weatherforecast`
- En consultant le répertoire de logs, on devrait trouver un fichier nommé comme expliqué plus haut, avec un layout du type :

```
2021-06-04 16:59:38.3531 INFO Message Info
2021-06-04 16:59:38.4224 DEBUG Message Debug
2021-06-04 16:59:38.4224 WARN Message Avertissement
2021-06-04 16:59:38.4224 ERROR Message Erreur
```

# Base de données

- On va créer la BDD par l'approche _code-first_ (ne dites rien à Clovis)
- Puis migrer la BDD vers un SGBDR (SQL Server) via les outils de migration d'Entity Framework

## Models

- Nouveau projet : `Entites`
- Répertoire `Models`
- 1 classe `Entreprise`, 1 classe `Employe`

```cs
public class Entreprise
{
	[Column("EntrepriseId")]
	public Guid Id { get; set; }

	[Required(ErrorMessage = "Le nom de l'entreprise est requis.")]
	[MaxLength(50, ErrorMessage = "La taille maximale pour le nom est de 50 caractères.")]
	public string Nom { get; set; }

	[Required(ErrorMessage = "L'adresse est requise.")]
	[MaxLength(80, ErrorMessage = "La taille maximale pour l'adresse est de 80 caractères.")]
	public string Adresse { get; set; }

	public string Pays { get; set; }

	public ICollection<Employe> Employes { get; set; }
}

public class Employe
{
	[Column("EmployeId")]
	public Guid Id { get; set; }

	[Required(ErrorMessage = "Le nom de l'employé est requis.")]
	[MaxLength(30, ErrorMessage = "La taille maximale pour le nom est de 30 caractères.")]
	public string Nom { get; set; }

	[Required(ErrorMessage = "L'âge est requis.")]
	public int Age { get; set; }

	[Required(ErrorMessage = "Le poste est requis.")]
	[MaxLength(20, ErrorMessage = "La taille maximale pour le poste est de 20 caractères.")]
	public string Poste { get; set; }

	[ForeignKey(nameof(Entreprise))]
	public Guid EntrepriseId { get; set; }

	public Entreprise entreprise { get; set; }
}
```

- L'annotation `Column` permet de spécifier un nom différent de celui de la propriété pour le schéma de la BDD
- La dernière propriété de chaque classe ne fait pas partie de l'entité (pas persistées) : elles ne servent que du point de vue de l'application, pour naviguer plus facilement

## Le contexte EF Core

- La classe de contexte (hérite de `DbContext`) est notre composant _middleware_ pour la communication avec la BDD
- EF Core : package à installer sur ce projet (`Microsoft.EntityFrameworkCore`)
- SQL Server : package à installer également (`Microsoft.EntityFrameworkCore.SqlServer`)
- Classe `RepoContext` à la racine du projet :

```cs
public class RepoContext : DbContext
{

	public RepoContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<Entreprise> Entreprises { get; set; }

	public DbSet<Employe> Employes { get; set; }
}
```

- Ajoutons une _connection string_ à `appsettings.json` :

```json
"ConnectionStrings": {
	"sqlConn": "server=(localdb)\\MSSQLLocalDB; database=SampleDb; Integrated Security=true"
}
```

- Via une méthode d'extension, permettre l'enregistrement du contexte `RepoContext` dans le container IoC, en précisant qu'on va utiliser SQL Server avec la _connection string_ spécifiée :

```cs
public static void ConfigureContextSql(this IServiceCollection services, IConfiguration config)
{
	services.AddDbContext<RepoContext>(opts => opts.UseSqlServer(config.GetConnectionString("sqlConn")));
}
```

- Enregistrement effectif du contexte depuis `Startup.ConfigureServices` :

```cs
services.ConfigureContextSql(Configuration);
```

## Migration vers SGBDR

- Pour migrer la DB, il va falloir dire dans quel _assembly_ (unité fondamentale de déploiement .NET, vos projets produisent des _assemblies_, par exemple)
- On modifie l'appel `UseSqlServer` pour spécifier le nom de cet assembly pour notre DB : `...opts.UseSqlServer(config.GetConnectionString("sqlConn"), opts => opts.MigrationsAssembly("LeNomDeVotreProjetPrincipal"))...`
- On aura aussi besoin du package `Microsoft.EntityFrameworkCore.Tools` dans `Entites` pour la migration (et possiblement de `Microsoft.EntityFrameworkCore.Design` dans le projet principal, à tester)
- Dans la console *Package Manager* : `Add-Migration CreationDB`
- Les fichiers de migration sont créés dans le projet principal (répertoire `Migrations`)
- Et on applique la migration avec la commande : `Update-Database`
- Vérifier le schéma, par exemple dans le panel _SQL Server Object Explorer_

## Jeu de données

- Dans `Entites`, créer un répertoire `Configs`
- Une classe `ConfigEntreprise`, une `ConfigEmploye`
- Une méthode `Configure` dans chaque classe dans laquelle on va créer un jeu de données :

```cs
public class ConfigEmploye : IEntityTypeConfiguration<Employe>
{
	public void Configure(EntityTypeBuilder<Employe> builder)
	{
		builder.HasData
		(
			new Employe { ... },
			new Employe { ... }
		);
	}
}
```

- Veillez à ce que les contraintes référentielles soient respectées : il faudra donner des Guid spécifiquess à vos entreprises afin que les objets `Employe` puissent les référencer dans la clé étrangère
- Ajouter la méthode suivante à la classe de contexte pour invoquer la création de ce jeu de données :

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
	builder.ApplyConfiguration(new ConfigEntreprise());
	builder.ApplyConfiguration(new ConfigEmploye());
}
```

- On va créer une autre migration pour envoyer ces données en DB : `Add-Migration JeuDeDonneesSimple`
- Et on l'applique : `Update-Database`
- Vérifier les insertions dans les tables

## Interface pour Repositories

- Création d'une interface CRUD pour tous nos repositories (dans `Contrats`, donc)

```cs
public interface IRepositoryBase<T>
{
	IQueryable<T> FindAll(bool tracked);
	IQueryable<T> FindByCondition(Expression<Func<T, bool>> expr, bool tracked);
	void Create(T entite);
	void Update(T entite);
	void Delete(T entite);
}
```

- Le paramètre `tracked` va nous permettre d'indiquer si on veut qu'EF « surveille » les éventuelles modifications sur l'entité
  - en le mettant à `false`, on va améliorer significativement les performances
  - cela pourra être fait que lorsqu'on fait un traitement en lecture seule
- Le paramètre `expr` va permettre d'indiquer le filtre de recherche
  - `Func<T, bool>` désigne une fonction qui prend un objet quelconque et renvoie un booléen : c'est la condition du filtre, ce qui va dire « on prend celui-ci, on prend pas celui-là »
  - `Expression<...>` va nous permettre d'exprimer cela sous forme de lambda expression dans le code, par exemple : `animal => animal.NbPattes < 1000` va filtrer sur les animaux qui ont moins de mille pattes

## _RepositoryBase_

- Nouveau projet `Repositories` (_Class Library_)
- Classe `RepositoryBase` qui implémente l'interface précédente
- C'est une classe abstraite, on ne veut pas l'instancier, on va encore dériver des sous-classes concrètes pour chacune de nos entités

```cs
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
	protected RepoContext Context;

	public RepositoryBase(RepoContext context)
	{
		Context = context;
	}

	public IQueryable<T> FindAll(bool tracked)
	{
		return tracked ? Context.Set<T>() : Context.Set<T>().AsNoTracking();
	}

	public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expr, bool tracked)
	{
		return tracked ? Context.Set<T>().Where(expr) : Context.Set<T>().Where(expr).AsNoTracking();
	}

	public void Create(T entite) => Context.Set<T>().Add(entite);

	public void Update(T entite) => Context.Set<T>().Update(entite);

	public void Delete(T entite) => Context.Set<T>().Remove(entite);

}
```

- La spécification de type générique `T` permet à notre classe d'être très flexible, elle peut ainsi s'adapter à n'importe quel modèle (entité)
- Noter l'utilisation de LINQ, et notamment de la clause `Where`, c'est elle qui permet finalement le filtrage en utilisant l'expression donnée en argument

## Interfaces pour les repositories

- On va d'abord créer de nouvelles interfaces pour nos tables, cela nous permettra d'avoir une logique d'accès aux données propre à chaque type d'entités
- Dans `Contrats`, interfaces `IEntrepriseRepo` et `IEmployeRepo`
- Vides pour l'instant, **on ajoutera des méthodes en fonction des besoins spécifiques pour l'accès à ces entités** (`GetEntreprises`, `Supprimer`...)

## Repositories concrets

- On revient dans le projet `Repositories`
- Création `EntrepriseRepo` et `EmployeRepo`, qui implémentent l'interface correspondante et dérivent de la classe abstraite `RepositoryBase`
- Les constructeurs appellent celui de la classe parente :

```cs
public EntrepriseRepo(RepoContext context) : base(context)
{
}
```

## Gestion de repositories

- Souvent les APIs renvoient des réponses qui contiennent des données provenant de sources multiples, et on a besoin de plusieurs repositories
- On peut les instancier à la volée au besoin
- Mais on va créer un gestionnaire de repositories, qui pourra créer des instances de repositories pour nous et les enregistrer dans le container IoC d'ASP.NET
- Cela nous permettra de résoudre les dépendances de repositories avec l'injection par constructeur d'un seul composant
- De plus, cette classe va s'occuper d'appeler la méthode `SaveChanges` pour nous à chaque opération qui le nécessitera (application effective des modification vers la BDD)
- Nouvelle interface `IGestionRepos` dans `Contrats`

```cs
public interface IGestionRepos
{
	IEntrepriseRepo Entreprises { get; }
	IEmployeRepo Employes { get; }
	void Save();
}
```

- Dans `Repositories`, on ajoute la classe `GestionRepos` :

```cs
public class GestionRepos : IGestionRepos
{
	private RepoContext _context;
	private IEntrepriseRepo _entrepriseRepo;
	private IEmployeRepo _employeRepo;

	public GestionRepos(RepoContext context)
	{
		_context = context;
	}

	public IEntrepriseRepo Entreprises
	{
		get
		{
			if (_entrepriseRepo == null)
			{
				_entrepriseRepo = new EntrepriseRepo(_context);
			}
			return _entrepriseRepo;
		}
	}

	public IEmployeRepo Employes
	{
		get
		{
			if (_employeRepo == null)
			{
				_employeRepo = new EmployeRepo(_context);
			}
			return _employeRepo;
		}
	}

	public void Save() => _context.SaveChanges();

}
```

- Les propriétés exposent les repositories concrets
- On s'assure qu'une seule instance de chaque repository est créée en appliquant le _pattern Singleton_ ; par la suite c'est toujours cette instance qui est renvoyée
- La méthode `Save` va nous permettre d'enregister d'un coup toutes les modifications qui pendouillent sur le contexte, au moment souhaité
- Si quelque chose cloche à ce moment, _toutes les modifications sont annulées_ (comme pour une transaction)
- Permettons l'enregistrement de cette classe dans l'IoC :

```cs
// ServiceExtensions.cs
public static void ConfigureGestionRepos(this IServiceCollection services) => services.AddScoped<IGestionRepos, GestionRepos>();
```

- Appliquer ensuite l'enregistrement effectif de cette règle d'injection
- On peut alors injecter un `GestionRepos` dans n'importe quel controller, ou dans n'importe quelle couche métier pour des applications larges (typiquement on aurait une couche métier entre les controllers et les repositories)

# Requêtes GET

## Routage

- Deux façons d'implémenter le routage :
  - par convention
  - par attribut `[...]`

## Routage par convention

- Voici un exemple de routage par convention :

```cs
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
		name: "default",
		pattern: "{controller=Root}/{action=Index}/{id?}");
});
```

- La première partie du pattern indique le nom du controller, la deuxième indique la méthode, et la troisième indique le paramètre optionnel
- Ce type de mapping est plutôt utilisé en mode MVC, on ne rentre pas dans le détail

## Routage par attribut

- Pour une API Web, on utilisera plutôt ce type de routage
- On utilise des attributs pour mapper les routes directement aux méthodes à l'intérieur du controller
- La route de base est spécifiée au niveau de la classe (ici par exemple `[Route("api/[controller]")]`)
- `[controller]` sera remplacé à la compilation par le nom de la classe du controller, _sans le suffixe `Controller`_
- Ainsi, `EntreprisesController` sera mappé au routage `api/entreprises`
- Les méthodes disposent également d'attributs qui viennent préciser la route
- On va illustrer ça tout de suite

## Exemple simple : récupérer toutes les entreprises

- Ajouter la méthode `GetEntreprises` à l'interface `IEntrepriseRepo`

```cs
IEnumerable<Entreprise> GetEntreprises(bool tracked);
```

- Implémenter la méthode dans `EntrepriseRepo` en utilisant le CRUD en place
- Ajouter une méthode `GetEntreprises` au controller
- Elle doit répondre directement à une requête sur `api/entreprises`, donc pas d'indication supplémentaire pour l'URI à ajouter en attribut
- En revanche, elle répond à un GET, on va donc lui indiquer l'attribut `[HttpGet]`
- Elle renvoie un `IActionResult`, concrètement ce sera un Status Code avec un éventuel contenu pour lequel ASP.NET propose différentes méthodes pour un accès facilité, par exemple :
  - `Ok(le_contenu)` pour renvoyer un `200` avec le contenu indiqué
  - `NotFound()` pour un `404`
  - `StatusCode(500, "Erreur serveur")`, méthode plus générique permettant de renvoyer n'importe quel code
- ASP.NET facilite la tâche à ce sujet mais, en règle générale, quel que soit l'environnement de développement, n'oubliez pas de renvoyer le _Status Code_ en plus du contenu éventuel
- En cas d'erreur (typiquement une exception lancée par l'appel à `GetEntreprises`), il faut logger l'erreur et retourner un 500
- On aura besoin d'injecter le gestionnaire de repositories ainsi que le logger pour faire tout ça

## Test

- On peut tester l'API avec un outil dédié quelconque
  - _Postman_ reste la référence du genre
  - pour des tests rapides si on ne veut pas quitter l'IDE, _Thunder Client_ sous VS Code est très bien fait (n'existe pas sous VS)
- Lancer l'application Web API
- Taper dessus avec votre outil : `https://localhost:5001/api/entreprises`
  - erreurs fréquentes : oubli du 's' sur https, oubli 'api', mauvaise orthographe sur le nom du controller (on oublie le s...) => d'abord vérifier ça
  - dans les settings de POSTMAN, décocher la vérification SSL si elle est activée, cela peut poser problème
- Vous devez récupérer une liste JSON contenant vos entreprises

```json
[
  {
    "id": "3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866",
    "nom": "Entreprise01 SA",
    "adresse": "Adresse01 chemin du 01",
    "pays": "FR",
    "employes": null
  },
  {
    "id": "378431c9-4b7c-4fb2-9263-abcac4167c09",
    "nom": "Entreprise02 SARL",
    "adresse": "Adresse02 rue du 02",
    "pays": "BE",
    "employes": null
  }
]
```

## Classes DTO (_Data Transfer Objects_)

- Un DTO est un objet utilisé pour stocker les données en transit
- Ce qu'on a fait précédemment n'est pas forcément considéré comme une bonne pratique, surtout pour des projets plus importants : on a renvoyé directement des entités dans la réponse, au lieu d'utiliser des DTO
- Pourquoi encore s'embêter avec une couche supplémentaire, sérieux ? On dirait un mille-feuilles
  - EF Core utilise des classes pour les modèles pour le mapping avec les tables en BDD : c'est le principe d'un modèle
  - nos modèles ont des propriétés de navigation (ex. : de `Employe` vers son `Enterprise`) et on ne veut pas forcément directement les exposer dans une réponse à une requête
  - plus important encore, les informations dont a besoin le client de l'API ne se mappent pas forcément immédiatement à une/des entité(s) ; cela peut être plus complexe (ou plus simple)
  - finalement, cela n'affecte pas le client qui s'attend à recevoir toujours les mêmes objets (les DTO) alors que la BDD sous-jacente change
- Conclusion : **avoir des DTO en plus des classes modèles est une bonne pratique**
- Projet `Entites`, répertoire `DTO`
- Classe `EntrepriseDTO`
- Cette classe va juste exposer 3 propriétés : `Id`, `Nom`, `AdresseComplete`
- Noter qu'elle ne se mappe donc pas directement au modèle pour s'adapter à des besoins-clients ; notamment, la relation vers `Employe` a disparu, et `AdresseComplete` a pour objet de regrouper l'adresse et le pays
- Pas d'attributs de validation non plus ici, c'est juste pour les réponses
- Modifier l'action `GetEntreprises` pour renvoyer des DTOs plutôt que la liste d'entités directement

- Le JSON renvoyé au client est ainsi plus « propre » et plus ciblé :

```json
[
  {
    "id": "3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866",
    "nom": "Entreprise01 SA",
    "adresseComplete": "Adresse01 chemin du 01 - FR"
  },
  {
    "id": "378431c9-4b7c-4fb2-9263-abcac4167c09",
    "nom": "Entreprise02 SARL",
    "adresseComplete": "Adresse02 rue du 02 - BE"
  }
]
```

- Il existe des outils qui permettent de faire à la demande le mapping entre l'entité et le DTO (ex. : _AutoMapper_)  ; c'est bien fait, et contrairement à ce qui se passe souvent avec ce genre d'outils qui « simplifient la vie », ça rend effectivement le code plus lisible et plus maintenable => très conseillé

## Une autre requête pour une ressource spécifique

- Nouvelle méthode pour l'interface `IEntrepriseRepo` :

```cs
Entreprise GetEntreprise(Guid id, bool tracked);
```

- Implémenter la méthode dans `EntrepriseRepo`, maintenant cassée
- Ajouter une méthode `GetEntreprise` dans le controller pour répondre à une requête du genre `api/entreprises/378431c9-4b7c-4fb2-9263-abcac4167c09`
- Cette méthode aura un attribut qui, en plus de spécifier le verbe HTTP, va indiquer la variable venant recevoir l'ID précisé : `[HttpGet({id}")]`
- La signature de la méthode précise la variable qui reçoit la valeur de l'id : `public IActionResult GetEntreprise(Guid id)`
- La méthode renvoie un 500 en cas d'erreur, un `404` si l'id ne correspond à aucune entreprise, et un DTO adéquat si tout se passe bien
- Tester sur `api/entreprises/un_id_qui_existe` et puis sur `api/entreprises/un_qui_n_existe_pas`

## Association Entreprise/Employe

- On veut accéder aux employés d'une entreprise donnée
- Créer un nouveau controller `EmployesController`
- Composer une URI adéquate
  - la route principale (annotation sur la classe) va comprendre l'id de l'entreprise souhaitée avec la même syntaxe que précédemment : `[Route("api/entreprises/{entrepriseId}/[controller]")]`
  - les méthodes de la classe vont pouvoir accéder à cette donnée exactement comme précédemment, sans rien spécifier sur le `[HttpGet]`
- Implémenter la totalité de la fonctionnalité, de l'interface jusqu'à la méthode du controller
- La méthode du controller doit renvoyer
  - 500 en cas de problème lors de la récupération de l'entreprise
  - `404` si l'entreprise n'existe pas
  - la liste des employés en DTO si tout se passe bien (sans l'id de l'entreprise)
- Tester avec une entreprise qui n'existe pas, avec une entreprise avec 0, 1 ou plusieurs employés

## Récupérer un employé spécifique d'une entreprise spécifique

- Fastoche. Go.
- Tester avec :
  - une entreprise qui n'existe pas
  - un employé qui n'existe pas
  - un employé qui existe mais pas dans cette entreprise
  - un cas où tout se passe bien

# POST - Création de ressources

## DTO pour les entrées

- On utilisera de nouveau le pattern DTO pour les entrées
- Recommandation : séparer les deux types de classes (DTO d'entrée et de sortie) pour des questions de maintenance ainsi que pour clarifier la partie validation des entrées
- `EntrepriseCreationDTO` :

```cs
public class EntrepriseCreationDTO
{
	public string Nom { get; set; }
	public string Adresse { get; set; }
	public string Pays { get; set; }
}
```

- Ajout d'une méthode dans l'interface `IEntrepriseRepo` :

```cs
void Creer(Entreprise entreprise);
```

- Implémentation très simple, elle délègue à `Create`
- EF Core crée bien sûr l'ID pour nous
- Côté controller, on va implémenter une méthode en `[HttpPost]` qui va prendre en paramètre un DTO d'entrée :

```cs
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
		Pays = entrepriseCreationDTO.Pays
	};

	_gestionRepos.Entreprises.Creer(entreprise);
	_gestionRepos.Save();

	var entrepriseRetour = new EntrepriseDTO
	{
		Id = entreprise.Id,
		Nom = entreprise.Nom,
		AdresseComplete = string.Join(' ', entreprise.Adresse, '-', entreprise.Pays)
	};

	return CreatedAtRoute("EntrepriseParId", new { id = entrepriseRetour.Id }, entrepriseRetour);
}
```

- `[FromBody]` indique que ce paramètre vient du contenu de la requête, pas de l'URI
- Le check à `null` est important, l'objet reçu pourrait ne pas être désérializable (pas de conversion vers objet correct possible)
- Ne pas oublier qu'on doit à un moment enregistrer les entités en DB, c'est ce que fait `Save()`
- La méthode renvoie au client la ressource dans une forme acceptable (ici on considère que `entrepriseDTO` est souhaité), comme toujours dans un _REST-POST_
- L'appel final pour la réponse introduit plusieurs choses :
  - `CreatedAtRoute` indique un `201` (_Created_)
  - `entrepriseRetour` : objet créé, renvoyé au client
  - `"EntrepriseParId"` : une sorte de « lien », désigne la méthode qui permettra de récupérer la ressource par la suite (cela va peupler la clé `Location` dans les _headers_ de la réponse)
  - `new { id = entrepriseRetour.Id }` : permet de peupler la partie dynamique (l'id) du lien généré précédemment
- Pour pouvoir relier cet `EntrepriseParId` à notre `GetEntreprise`, on doit lui ajouter une propriété `Name` dans le `HttpGet` :

```cs
[HttpGet("{id}", Name = "EntrepriseParId")]
public IActionResult GetEntreprise(Guid id) { ... }
```

- Tester sous Postman avec une requête contenant des données d'entreprise valides en JSON :

```json
{
  "nom": "Entreprise 03",
  "adresse": "Adresse 03 bd du 03",
  "pays": "FR"
}
```

- Vérifier que la réponse reçue est un `201` et qu'elle contient l'objet créé
- Vérifier la clé `Location` dans les _headers_
- Copier/coller et tester le lien de `Location` pour tester qu'il est correct
- Vérifier la non-idempotence de cette opération en l'exécutant plusieurs fois avec la même donnée d'entrée pour s'assurer que plusieurs ressources avec différents ID sont effectivement créées

## Création de ressource fille

- Implémenter la classe `EmployeCreationDTO`
- Le `EntrepriseId` ne fait pas partie du DTO d'entrée : cette information est en effet spécifiée _par l'URI_ lors du POST (`api/entreprises/{entrepriseId}/employes`)
- Modifier l'interface `IEmployeRepository` avec la méthode `CreerPourEntreprise` :

```cs
void CreerPourEntreprise(Guid entrepriseId, Employe employe);
```

- Implémenter la méthode correspondante
- Implémenter le POST dans le controller
  - ne pas oublier de vérifier que l'entreprise spécifiée existe (`404` sinon)
  - dans l'appel pour l'envoi de la réponse, il faudra spécifier un objet anonyme `{ ... }` contenant deux informations, et pas une seule comme précédemment
- Tester
  - avec une entreprise qui n'existe pas
  - et une qui existe
  - vérifier la conformité du lien GET renvoyé par la réponse
  - vérifier la non-idempotence du POST

# Implémentation DELETE

## Suppression d'un employé

- On veut supprimer un employé à partir de son ID
- Réaliser l'implémentation complète
- _Hints_
  - bien définir la route et la méthode HTTP associée
  - dans le controller, vérifier l'existence de l'entreprise spécifiée
  - vérifier l'existence de l'employé
  - ne pas oublier le logging des erreurs
  - on renvoie un `NoContent()` (c'est un `204 No Content`)
- Tester en vérifiant tous les cas de figure comme d'habitude
  - entreprise inconnue
  - employé inconnu
  - employé supprimé a-t-il effectivement disparu ?
  - DELETE est supposée idempotente ; vérifier (`204` pour la première requête, `404` pour les requêtes identiques suivantes : l'état du serveur a changé lors de la _première_ requête, et par la suite la même requête ne l'affecte plus du tout)

## Suppression parent + enfants

- Suppression entreprise => suppression de ses employés
- *Cascade Deleting* : activé par défaut sous EF Core, permet de supprimer les ressources filles automatiquement (modifier le fichier de migration avant application, par exemple, permet de paramétrer ce comportement : chercher `OnDelete` dans le code généré)
- Donc ici _suppression entreprise => suppression automatique de ses employés_
- Rien de neuf pour le reste
- Implémenter
- Tester comme d'habitude ; vérifier la suppression automatique des enfants

# Implémentation PUT

## Mise à jour employé

- Créer un DTO `EmployeUpdateDTO`, sans propriété `Id` (info fournie par URI, comme pour DELETE)
- Sinon, mêmes propriétés que `EmployeCreationDTO`, ce qui en fait deux concepts différents, même si ça ressemble à de la duplication

  - les besoins pourraient être différents par la suite, cela permet d'anticiper à moindre coôt
  - pour la validation, d'autres différences pourront exister aussi

- Implémenter la modification d'employé dans le controller (`MiseAJourEmployeDeLEntreprise`)
  - on reçoit un DTO depuis le contenu (`FromBody`)
  - on recherche l'entreprise (`404` si inconnue)
  - on recherche l'employé (`404` si inconnu)
  - **cette fois, on voudra `tracked` à `true`** :
    - EF Core doit surveiller les modifications faites à l'entité renvoyée par la recherche
    - dès qu'une modification sera faite, EF Core va passer l'état de cette entité à `Modified`
    - au moment de sauvegarder le contexte, l'entité sera alors bien prise en compte, bien qu'elle existait déjà
  - on met à jour l'employé (ici on est en mode « connecté », on peut directement modifier l'objet, pas besoin d'appeler `Update` sur le repository)
  - et on `Save`
  - deux possibilités pour le renvoi au client, selon les besoins :
    - on renvoie la ressource modifiée avec un `200`
    - on renvoie un `204` sans contenu
- Si on veut voir les requêtes SQL qu'EF exécute effectivement, il faut observer la console de sortie ; si on ne voit rien, on peut ajouter `"Microsoft.EntityFrameworkCore": "Information"` dans la section `LogLevel` de `appsettings.json`
- Tester, par exemple en modifiant l'âge d'un employé (`200` ou `204`, donc), et bien sûr avec une entreprise invalide et un employé invalide (`404` attendu)
- **Attention**, une requête PUT doit toujours envoyer **toutes** les informations de la ressource à mettre à jour
  - que ce soit par l'URI, les _headers_, le contenu, le serveur devrait toujours reconstituer l'intégralité de l'objet à partir de la requête avant d'effectuer la modification
  - si ce n'est pas fait, avec une API REST, il faut s'attendre à ce que les champs qui ne sont pas renseignés soient mis à des valeurs par défaut (ce qui n'est certainement pas ce que l'on souhaite)
  - **PUT est une mise à jour complète de la ressource** (contrairement à PATCH)

## Pourquoi n'a-t-on pas utilisé la méthode `Update` ?

- On a utilisé le même objet contexte pour récupérer l'entité et pour la mettre à jour, donc ici on n'a pas eu besoin d'utiliser la méthode `Update` du repository
- Si on utilise des objets contexte différents pour récupérer et mettre à jour la base (par le jeu de méthodes/classes différentes), on utilisera alors `Update`
- Parfois, le contenu de la requête inclut la propriété ID de la ressource à mettre à jour (cela évite d'avoir à la récupérer dans la BDD) ; dans ce cas, on utilisera également `Update` pour la mise à jour
- Dans tous les cas, on n'oublie pas de préciser à EF de surveiller les modifications sur l'entité (`tracked` à `true`) et on n'oublie pas non plus que _toutes_ les propriétés seront potentiellement mises à jour par l'update (même si on change juste une seule propriété)

# Implémentation PATCH

## Principe

- Rappel :
  - **PUT** met à jour la ressource **complètement**
  - **PATCH** met à jour une **partie** de la ressource
- Différences concernant ASP.NET Core :
  - dans le controller, changement dans le traitement des requêtes, par exemple : `[FromBody] Entreprise` deviendrait `[FromBody] JsonPatchDocument<Entreprise>`
  - dans le _header_ de la requête, `application/json` deviendrait `application/json-patch+json` (pas absolument nécessaire, mais recommandé)
- Voici un exemple de contenu d'une requête PATCH :

```json
[
  {
    "op": "replace",
    "path": "/nom",
    "value": "nouveau nom"
  },
  {
    "op": "remove",
    "path": "/nom"
  }
]
```

- C'est donc un tableau qui représente une liste d'opérations à effectuer
- Ici, la première opération remplace la valeur de la propriété `nom` par `nouveau nom`
- La deuxième supprime la propriété `nom` (qui prendra une valeur par défaut)

- Voici les six opérations (`op`) possibles pour un PATCH :

1. **add** : affecte une nouvelle valeur (`value`) à une propriété (`path`, existante ou non)
2. **remove** : réinitialise une propriété (`path`) à sa valeur par défaut
3. **replace** : équivalent à `remove` + `add`
4. **copy** : copie la valeur de la propriété source (`from`) vers la propriété cible (`path`)
5. **move** : `remove` de propriété source (`from`) + `add` à la propriété cible (`path`)
6. **test** : renvoie OK si la valeur spécifiée (`value`) correspond à la valeur de la propriété spécifiée (`path`)

## Prérequis

- Deux bibliothèques sont nécessaires :
  - `Microsoft.AspNetCore.JsonPatch` : pour le support de `JsonPatchDocument`, vu plus haut
  - `Microsoft.AspNetCore.Mvc.NewtonsoftJson` : si pas déjà installée ; support de la conversion en `PatchDocument` (même si ASP.NET Core 5 supporte naturellement Json, ce package spécialisé reste nécessaire pour certains types d'utilisation, comme PATCH)
- Ajout de `NewtonsoftJson` dans la chaîne d'initialisation des services, pour indiquer qu'on va l'utiliser :

```c#
// Startup.cs
services.AddControllers().AddNewtonsoftJson();
```

## Patch sur Employe

- Avec tout ça en place, on va pouvoir écrire une méthode pour répondre à un PATCH sur un employé
  - `HttpPatch` en récupérant l'id de l'URI : `[HttpPatch("{employeId}")]`
  - en plus des IDs de l'entreprise et de l'employé, récupération en paramètre du `JsonPatchDocument` qui va contenir les infos du patch (opérations, etc.) sous forme de DTO (on peut réutiliser ici le DTO pour la mise à jour, déjà existant)
  - on va récupérer du repo l'entité "employé" correspondante, créer un DTO à partir de cette entité, appliquer le patch sur ce DTO puis remapper ce DTO à l'entité "employé" avant de sauvegarder
  - encore une fois, si on se passe d'un DTO, il y aura moins de code mais ce sera moins flexible
  - on verra une manière d'avoir le meilleur des deux mondes

```csharp
[HttpPatch("{employeId}")]
public IActionResult MettreAJourPartiellementEmployeDeLEntreprise(Guid entrepriseId, Guid employeId, [FromBody] JsonPatchDocument<EmployeUpdateDTO> patchDoc)
{
	if (patchDoc == null)
	{
		_logger.LogErreur("EmployeUpdateDTO reçu du client est null.");
		return BadRequest("Objet EmployeUpdateDTO envoyé est null.");
	}

	var entreprise = _gestionRepos.Entreprises.GetEntreprise(entrepriseId, tracked: false);
	if (entreprise == null)
	{
		_logger.LogInfo($"L'entreprise avec l'id: {entrepriseId} n'existe pas en BDD.");
		return NotFound();
	}

	var employe = _gestionRepos.Employes.GetEmployeDeLEntreprise(entrepriseId, employeId, tracked: true);
	if (employe == null)
	{
		_logger.LogInfo($"L'employé avec l'id: {employeId} n'existe pas en BDD.");
		return NotFound();
	}

	var employeDTOPatched = new EmployeUpdateDTO
	{
		Nom = employe.Nom,
		Age = employe.Age,
		Poste = employe.Poste
	};

	patchDoc.ApplyTo(employeDTOPatched);
	employe.Nom = employeDTOPatched.Nom;
	employe.Age = employeDTOPatched.Age;
	employe.Poste = employeDTOPatched.Poste;

	_gestionRepos.Save();

	return NoContent();
}
```

- On teste en essayant par exemple d'envoyer plusieurs requêtes PATCH :
  - modif de l'âge d'un employé (attention, même si on ne teste qu'une seule opération à la fois, il faut envoyer un tableau JSON)
  - suppression de l'âge (donc mise à 0)
  - ajout de l'âge (`add` remplace si existe, ajoute si n'existe pas)

```json
[
  {
    "op": "replace",
    "path": "/age",
    "value": "31"
  }
]
```
