-- ===========================
-- Database initialization script
-- ===========================

-- ===========================
-- Sequence
-- ===========================
CREATE SEQUENCE IF NOT EXISTS public."Movies_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."UserFavorites_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."UserRatings_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."Users_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."People_Id_seq";

-- ===========================
-- Table Users
-- ===========================
CREATE TABLE IF NOT EXISTS public."Users"
(
    "Id" integer NOT NULL DEFAULT nextval('"Users_Id_seq"'::regclass),
    "Name" character varying(100) NOT NULL,
    "Email" character varying(200) NOT NULL,
    "PasswordHash" text NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLogin" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DeletionDate" timestamp without time zone,
    CONSTRAINT "Users_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "Users_Email_key" UNIQUE ("Email")
) TABLESPACE pg_default;

ALTER TABLE public."Users" OWNER TO your_database_user;

-- ===========================
-- Table Movies 
-- ===========================
CREATE TABLE IF NOT EXISTS public."Movies"
(
    "Id" integer NOT NULL DEFAULT nextval('"Movies_Id_seq"'::regclass),
    "TMDbId" integer NOT NULL,
    "Title" character varying(255) COLLATE pg_catalog."default" NOT NULL,
    "Overview" text COLLATE pg_catalog."default",
    "ReleaseDate" timestamp without time zone,
    "PosterPath" character varying(500) COLLATE pg_catalog."default",
    "VoteAverage" double precision NOT NULL DEFAULT 0,
    "VoteCount" integer NOT NULL DEFAULT 0,
    "LastUpdated" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "BackdropPath" character varying(500) COLLATE pg_catalog."default",
    CONSTRAINT "Movies_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "Movies_TMDbId_key" UNIQUE ("TMDbId")
) TABLESPACE pg_default;

ALTER TABLE public."Movies" OWNER TO your_database_user;

-- ===========================
-- Table People 
-- ===========================
CREATE TABLE IF NOT EXISTS public."People"
(
    "Id" integer NOT NULL DEFAULT nextval('"People_Id_seq"'::regclass),
    "TMDbId" integer,
    "Name" character varying(255) COLLATE pg_catalog."default" NOT NULL,
    "Biography" text COLLATE pg_catalog."default",
    "ProfilePath" character varying(255) COLLATE pg_catalog."default",
    "Birthday" timestamp without time zone,
    "Deathday" timestamp without time zone,
    "PlaceOfBirth" character varying(255) COLLATE pg_catalog."default",
    "LastUpdated" timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "People_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "People_TMDbId_key" UNIQUE ("TMDbId")
) TABLESPACE pg_default;

ALTER TABLE public."People" OWNER TO postgres;

-- ===========================
-- Table MoviePeople 
-- ===========================
CREATE TABLE IF NOT EXISTS public."MoviePeople"
(
    "MovieId" integer NOT NULL,
    "PersonId" integer NOT NULL,
    "Role" integer NOT NULL,
    "Character" character varying(255) COLLATE pg_catalog."default",
    "Order" integer,
    CONSTRAINT "MoviePeople_pkey" PRIMARY KEY ("MovieId", "PersonId", "Role"),
    CONSTRAINT "MoviePeople_MovieId_fkey" FOREIGN KEY ("MovieId")
        REFERENCES public."Movies" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT "MoviePeople_PersonId_fkey" FOREIGN KEY ("PersonId")
        REFERENCES public."People" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE
) TABLESPACE pg_default;

ALTER TABLE public."MoviePeople" OWNER TO your_database_user;

-- ===========================
-- Table UserFavorites
-- ===========================
CREATE TABLE IF NOT EXISTS public."UserFavorites"
(
    "Id" integer NOT NULL DEFAULT nextval('"UserFavorites_Id_seq"'::regclass),
    "UserId" integer NOT NULL,
    "MovieId" integer NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DeletionDate" timestamp without time zone,
    CONSTRAINT "UserFavorites_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT uq_user_movie UNIQUE ("UserId", "MovieId"),
    CONSTRAINT fk_userfavorites_user FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT fk_userfavorites_movie FOREIGN KEY ("MovieId")
        REFERENCES public."Movies" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE
) TABLESPACE pg_default;

ALTER TABLE public."UserFavorites" OWNER TO your_database_user;

-- ===========================
-- Table UserRatings
-- ===========================
CREATE TABLE IF NOT EXISTS public."UserRatings"
(
    "Id" integer NOT NULL DEFAULT nextval('"UserRatings_Id_seq"'::regclass),
    "UserId" integer NOT NULL,
    "MovieId" integer NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" text,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp without time zone,
    "DeletionDate" timestamp without time zone,
    "LastActivityDate" timestamp without time zone,
    CONSTRAINT "UserRatings_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT uq_user_rating UNIQUE ("UserId", "MovieId"),
    CONSTRAINT fk_userratings_user FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT fk_userratings_movie FOREIGN KEY ("MovieId")
        REFERENCES public."Movies" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT "UserRatings_Rating_check" CHECK ("Rating" >= 1 AND "Rating" <= 10)
) TABLESPACE pg_default;

ALTER TABLE public."UserRatings" OWNER TO your_database_user;