using CineHub.Models.Enum;

namespace CineHub.Extensions
{
    public static class PersonRoleExtensions
    {
        private static readonly Dictionary<PersonRole, string> _roleDisplayNames = new()
        {
            // CAST
            [PersonRole.Actor] = "Ator",

            // DIRECTING DEPARTMENT
            [PersonRole.Director] = "Diretor",
            [PersonRole.AssistantDirector] = "Assistente de Direção",
            [PersonRole.SecondAssistantDirector] = "2º Assistente de Direção",
            [PersonRole.ThirdAssistantDirector] = "3º Assistente de Direção",
            [PersonRole.ScriptSupervisor] = "Supervisor de Roteiro",

            // WRITING DEPARTMENT
            [PersonRole.Writer] = "Roteirista",
            [PersonRole.Screenplay] = "Roteiro",
            [PersonRole.Story] = "História",
            [PersonRole.Characters] = "Personagens",
            [PersonRole.Novel] = "Romance",
            [PersonRole.Book] = "Livro",

            // PRODUCTION DEPARTMENT
            [PersonRole.Producer] = "Produtor",
            [PersonRole.ExecutiveProducer] = "Produtor Executivo",
            [PersonRole.CoProducer] = "Coprodutor",
            [PersonRole.AssociateProducer] = "Produtor Associado",
            [PersonRole.LineProducer] = "Produtor de Linha",
            [PersonRole.CastingDirector] = "Diretor de Elenco",
            [PersonRole.ProductionManager] = "Gerente de Produção",
            [PersonRole.UnitProductionManager] = "Gerente de Unidade de Produção",
            [PersonRole.LocationManager] = "Gerente de Locação",

            // CAMERA DEPARTMENT
            [PersonRole.DirectorOfPhotography] = "Diretor de Fotografia",
            [PersonRole.Cinematographer] = "Cinematógrafo",
            [PersonRole.CameraOperator] = "Operador de Câmera",
            [PersonRole.FirstAssistantCamera] = "1º Assistente de Câmera",
            [PersonRole.SecondAssistantCamera] = "2º Assistente de Câmera",
            [PersonRole.SteadicamOperator] = "Operador de Steadicam",

            // EDITING DEPARTMENT
            [PersonRole.Editor] = "Editor",
            [PersonRole.AssistantEditor] = "Assistente de Edição",
            [PersonRole.ColoringEditor] = "Editor de Cor",

            // SOUND DEPARTMENT
            [PersonRole.SoundDesigner] = "Designer de Som",
            [PersonRole.SoundMixer] = "Mixador de Som",
            [PersonRole.SoundRecordist] = "Técnico de Som",
            [PersonRole.BoomOperator] = "Operador de Boom",
            [PersonRole.SoundEditor] = "Editor de Som",
            [PersonRole.DialogueEditor] = "Editor de Diálogo",
            [PersonRole.SoundEffectsEditor] = "Editor de Efeitos Sonoros",

            // MUSIC DEPARTMENT
            [PersonRole.Composer] = "Compositor",
            [PersonRole.MusicSupervisor] = "Supervisor Musical",
            [PersonRole.Conductor] = "Maestro",
            [PersonRole.Orchestrator] = "Orquestrador",

            // ART DEPARTMENT
            [PersonRole.ProductionDesigner] = "Designer de Produção",
            [PersonRole.ArtDirector] = "Diretor de Arte",
            [PersonRole.SetDecorator] = "Decorador de Cenário",
            [PersonRole.PropertyMaster] = "Mestre de Objetos",
            [PersonRole.SetDesigner] = "Designer de Cenário",
            [PersonRole.ConceptArtist] = "Artista Conceitual",

            // COSTUME & MAKE-UP DEPARTMENT
            [PersonRole.CostumeDesigner] = "Designer de Figurino",
            [PersonRole.CostumeDesign] = "Design de Figurino",
            [PersonRole.MakeupArtist] = "Maquiador",
            [PersonRole.HairDesigner] = "Designer de Cabelo",
            [PersonRole.KeyMakeupArtist] = "Maquiador Principal",

            // VISUAL EFFECTS DEPARTMENT
            [PersonRole.VisualEffectsSupervisor] = "Supervisor de Efeitos Visuais",
            [PersonRole.VisualEffects] = "Efeitos Visuais",
            [PersonRole.VisualEffectsArtist] = "Artista de Efeitos Visuais",
            [PersonRole.VisualEffectsCoordinator] = "Coordenador de Efeitos Visuais",

            // LIGHTING DEPARTMENT
            [PersonRole.Gaffer] = "Chefe de Iluminação",
            [PersonRole.BestBoyElectric] = "Assistente de Iluminação",
            [PersonRole.Electrician] = "Eletricista",

            // CREW (GENERAL)
            [PersonRole.Stunts] = "Dublê",
            [PersonRole.StuntCoordinator] = "Coordenador de Dublês",
            [PersonRole.SecondUnitDirector] = "Diretor de Segunda Unidade",
            [PersonRole.UnitManager] = "Gerente de Unidade",

            // SPECIAL ROLES
            [PersonRole.Narrator] = "Narrador",
            [PersonRole.Presenter] = "Apresentador",
            [PersonRole.Host] = "Anfitrião",

            // GENERIC/FALLBACK
            [PersonRole.CrewMember] = "Membro da Equipe",
            [PersonRole.Other] = "Outros"
        };

        private static readonly Dictionary<PersonRole, int> _roleImportance = new()
        {
            [PersonRole.Director] = 1,
            [PersonRole.Writer] = 2,
            [PersonRole.Screenplay] = 3,
            [PersonRole.Story] = 4,
            [PersonRole.Producer] = 5,
            [PersonRole.ExecutiveProducer] = 6,
            [PersonRole.CoProducer] = 7,
            [PersonRole.Novel] = 8,
            [PersonRole.Book] = 9,
            [PersonRole.Actor] = 10,
            [PersonRole.DirectorOfPhotography] = 11,
            [PersonRole.Cinematographer] = 12,
            [PersonRole.Editor] = 13,
            [PersonRole.Composer] = 14,
            [PersonRole.ProductionDesigner] = 15,
            [PersonRole.CostumeDesigner] = 16
        };

        // Sets of roles for categorization 
        public static readonly PersonRole[] DirectorRoles = { PersonRole.Director };

        public static readonly PersonRole[] WriterRoles =
        {
            PersonRole.Writer, PersonRole.Screenplay, PersonRole.Story,
            PersonRole.Novel, PersonRole.Book
        };

        public static readonly PersonRole[] ProducerRoles =
        {
            PersonRole.Producer, PersonRole.ExecutiveProducer, PersonRole.CoProducer
        };

        public static readonly PersonRole[] ActorRoles = { PersonRole.Actor };

        // CORRIGIDO: MainRoles agora contém apenas os roles que você especificou
        public static readonly PersonRole[] MainRoles = DirectorRoles
            .Concat(WriterRoles)
            .Concat(ProducerRoles)
            .Concat(ActorRoles)
            .ToArray();

        // Returns the Portuguese display name for the role
        public static string GetDisplayName(this PersonRole role)
        {
            return _roleDisplayNames.TryGetValue(role, out var displayName)
                ? displayName
                : role.ToString().Replace("_", " ");
        }

        // Returns the importance of the role (lower number = more important)
        public static int GetImportance(this PersonRole role)
        {
            return _roleImportance.TryGetValue(role, out var importance) ? importance : 99;
        }

        // Checks if the role is a main role (appears in dedicated sections)
        public static bool IsMainRole(this PersonRole role)
        {
            return MainRoles.Contains(role);
        }

        // Checks if the role is a director role
        public static bool IsDirectorRole(this PersonRole role)
        {
            return DirectorRoles.Contains(role);
        }

        // Checks if the role is a writer role
        public static bool IsWriterRole(this PersonRole role)
        {
            return WriterRoles.Contains(role);
        }

        // Checks if the role is a producer role
        public static bool IsProducerRole(this PersonRole role)
        {
            return ProducerRoles.Contains(role);
        }

        // Checks if the role is an actor role
        public static bool IsActorRole(this PersonRole role)
        {
            return ActorRoles.Contains(role);
        }

        // Maps a TMDb job to a PersonRole
        public static PersonRole MapFromJob(string job)
        {
            if (string.IsNullOrWhiteSpace(job))
                return PersonRole.CrewMember;

            // Normalize job for comparison
            var normalizedJob = job.Trim().ToLowerInvariant();

            return normalizedJob switch
            {
                // DIRECTING
                "director" => PersonRole.Director,
                "assistant director" => PersonRole.AssistantDirector,
                "second assistant director" => PersonRole.SecondAssistantDirector,
                "third assistant director" => PersonRole.ThirdAssistantDirector,
                "script supervisor" => PersonRole.ScriptSupervisor,

                // WRITING
                "writer" => PersonRole.Writer,
                "screenplay" => PersonRole.Screenplay,
                "story" => PersonRole.Story,
                "characters" => PersonRole.Characters,
                "novel" => PersonRole.Novel,
                "book" => PersonRole.Book,

                // PRODUCTION
                "producer" => PersonRole.Producer,
                "executive producer" => PersonRole.ExecutiveProducer,
                "co-producer" => PersonRole.CoProducer,
                "associate producer" => PersonRole.AssociateProducer,
                "line producer" => PersonRole.LineProducer,
                "casting director" => PersonRole.CastingDirector,
                "production manager" => PersonRole.ProductionManager,
                "unit production manager" => PersonRole.UnitProductionManager,
                "location manager" => PersonRole.LocationManager,

                // CAMERA
                "director of photography" => PersonRole.DirectorOfPhotography,
                "cinematographer" => PersonRole.Cinematographer,
                "camera operator" => PersonRole.CameraOperator,
                "first assistant camera" => PersonRole.FirstAssistantCamera,
                "second assistant camera" => PersonRole.SecondAssistantCamera,
                "steadicam operator" => PersonRole.SteadicamOperator,

                // EDITING
                "editor" => PersonRole.Editor,
                "assistant editor" => PersonRole.AssistantEditor,
                "coloring editor" or "color editor" => PersonRole.ColoringEditor,

                // SOUND
                "sound designer" => PersonRole.SoundDesigner,
                "sound mixer" => PersonRole.SoundMixer,
                "sound recordist" => PersonRole.SoundRecordist,
                "boom operator" => PersonRole.BoomOperator,
                "sound editor" => PersonRole.SoundEditor,
                "dialogue editor" => PersonRole.DialogueEditor,
                "sound effects editor" => PersonRole.SoundEffectsEditor,

                // MUSIC
                "music" or "original music composer" or "composer" => PersonRole.Composer,
                "music supervisor" => PersonRole.MusicSupervisor,
                "conductor" => PersonRole.Conductor,
                "orchestrator" => PersonRole.Orchestrator,

                // ART
                "production designer" => PersonRole.ProductionDesigner,
                "art director" => PersonRole.ArtDirector,
                "set decorator" => PersonRole.SetDecorator,
                "property master" => PersonRole.PropertyMaster,
                "set designer" => PersonRole.SetDesigner,
                "concept artist" => PersonRole.ConceptArtist,

                // COSTUME & MAKEUP
                "costume designer" => PersonRole.CostumeDesigner,
                "costume design" => PersonRole.CostumeDesign,
                "makeup artist" => PersonRole.MakeupArtist,
                "hair designer" => PersonRole.HairDesigner,
                "key makeup artist" => PersonRole.KeyMakeupArtist,

                // VISUAL EFFECTS
                "visual effects supervisor" => PersonRole.VisualEffectsSupervisor,
                "visual effects" => PersonRole.VisualEffects,
                "visual effects artist" => PersonRole.VisualEffectsArtist,
                "visual effects coordinator" => PersonRole.VisualEffectsCoordinator,

                // LIGHTING
                "gaffer" => PersonRole.Gaffer,
                "best boy electric" => PersonRole.BestBoyElectric,
                "electrician" => PersonRole.Electrician,

                // STUNTS
                "stunts" => PersonRole.Stunts,
                "stunt coordinator" => PersonRole.StuntCoordinator,
                "second unit director" => PersonRole.SecondUnitDirector,
                "unit manager" => PersonRole.UnitManager,

                // SPECIAL
                "narrator" => PersonRole.Narrator,
                "presenter" => PersonRole.Presenter,
                "host" => PersonRole.Host,

                // DEFAULT
                _ => PersonRole.CrewMember
            };
        }

        public static bool ShouldIncludeJob(string job)
        {
            if (string.IsNullOrWhiteSpace(job))
                return false;

            // Jobs that are generally not relevant for display
            var excludedJobs = new[]
            {
                "thanks",
                "special thanks",
                "additional thanks",
                "archival footage",
                "stock footage",
                "archive footage"
            };

            return !excludedJobs.Contains(job.ToLowerInvariant());
        }
    }
}