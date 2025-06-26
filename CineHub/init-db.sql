-- ===========================
-- Script de inicialização do banco (GENÉRICO)
-- ===========================

-- ===========================
-- Sequências
-- ===========================
CREATE SEQUENCE IF NOT EXISTS public."Movies_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."UserFavorites_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."UserRatings_Id_seq";
CREATE SEQUENCE IF NOT EXISTS public."Users_Id_seq";

-- ===========================
-- Tabela Users
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
-- Tabela Movies
-- ===========================
CREATE TABLE IF NOT EXISTS public."Movies"
(
    "Id" integer NOT NULL DEFAULT nextval('"Movies_Id_seq"'::regclass),
    "TMDbId" integer NOT NULL,
    "Title" character varying(255) NOT NULL,
    "Overview" text,
    "ReleaseDate" timestamp without time zone,
    "PosterPath" character varying(500),
    "VoteAverage" double precision NOT NULL DEFAULT 0,
    "VoteCount" integer NOT NULL DEFAULT 0,
    "LastUpdated" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Movies_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "Movies_TMDbId_key" UNIQUE ("TMDbId")
) TABLESPACE pg_default;

ALTER TABLE public."Movies" OWNER TO your_database_user;

-- ===========================
-- Tabela UserFavorites
-- ===========================
CREATE TABLE IF NOT EXISTS public."UserFavorites"
(
    "Id" integer NOT NULL DEFAULT nextval('"UserFavorites_Id_seq"'::regclass),
    "UserId" integer NOT NULL,
    "MovieId" integer NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DeletionDate" timestamp without time zone,
    CONSTRAINT "UserFavorites_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "UserFavorites_User_Movie_key" UNIQUE ("UserId","MovieId"),
    CONSTRAINT "fk_userfavorites_user" FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT "fk_userfavorites_movie" FOREIGN KEY ("MovieId")
        REFERENCES public."Movies" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE
) TABLESPACE pg_default;

ALTER TABLE public."UserFavorites" OWNER TO your_database_user;

-- ===========================
-- Tabela UserRatings
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
    CONSTRAINT "UserRatings_User_Movie_key" UNIQUE ("UserId","MovieId"),
    CONSTRAINT "fk_userratings_user" FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT "fk_userratings_movie" FOREIGN KEY ("MovieId")
        REFERENCES public."Movies" ("Id") ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT "UserRatings_Rating_check" CHECK ("Rating" >= 1 AND "Rating" <= 10)
) TABLESPACE pg_default;

ALTER TABLE public."UserRatings" OWNER TO your_database_user;