namespace CineHub.Models.Enum
{
    public enum PersonRole
    {
        // CAST
        Actor,

        // DIRECTING DEPARTMENT
        Director,
        AssistantDirector,
        SecondAssistantDirector,
        ThirdAssistantDirector,
        ScriptSupervisor,

        // WRITING DEPARTMENT
        Writer,
        Screenplay,
        Story,
        Characters,
        Novel,
        Book,

        // PRODUCTION DEPARTMENT
        Producer,
        ExecutiveProducer,
        CoProducer,
        AssociateProducer,
        LineProducer,
        CastingDirector,
        ProductionManager,
        UnitProductionManager,
        LocationManager,

        // CAMERA DEPARTMENT
        DirectorOfPhotography,
        Cinematographer,
        CameraOperator,
        FirstAssistantCamera,
        SecondAssistantCamera,
        SteadicamOperator,

        // EDITING DEPARTMENT
        Editor,
        AssistantEditor,
        ColoringEditor,

        // SOUND DEPARTMENT
        SoundDesigner,
        SoundMixer,
        SoundRecordist,
        BoomOperator,
        SoundEditor,
        DialogueEditor,
        SoundEffectsEditor,

        // MUSIC DEPARTMENT
        Composer,
        MusicSupervisor,
        Conductor,
        Orchestrator,

        // ART DEPARTMENT
        ProductionDesigner,
        ArtDirector,
        SetDecorator,
        PropertyMaster,
        SetDesigner,
        ConceptArtist,

        // COSTUME & MAKE-UP DEPARTMENT
        CostumeDesigner,
        CostumeDesign,
        MakeupArtist,
        HairDesigner,
        KeyMakeupArtist,

        // VISUAL EFFECTS DEPARTMENT
        VisualEffectsSupervisor,
        VisualEffects,
        VisualEffectsArtist,
        VisualEffectsCoordinator,

        // LIGHTING DEPARTMENT
        Gaffer,
        BestBoyElectric,
        Electrician,

        // CREW (GENERAL)
        Stunts,
        StuntCoordinator,
        SecondUnitDirector,
        UnitManager,

        // SPECIAL ROLES
        Narrator,
        Presenter,
        Host,

        // GENERIC/FALLBACK
        CrewMember,
        Other
    }
}