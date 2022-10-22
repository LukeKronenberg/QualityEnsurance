--
-- PostgreSQL database dump
--

-- Dumped from database version 14.2
-- Dumped by pg_dump version 14.4

-- Started on 2022-10-22 23:00:10

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE ONLY public.guild_activity_user DROP CONSTRAINT guild_activity_user_guild_activity_guild_id_guild_activity_fkey;
ALTER TABLE ONLY public.guild_activity_user DROP CONSTRAINT guild_activity_user_guild_activity_guild_id_fkey;
ALTER TABLE ONLY public.guild_activity_user DROP CONSTRAINT guild_activity_user_guild_activity_activity_id_fkey;
ALTER TABLE ONLY public.guild_activity DROP CONSTRAINT guild_activity_guild_id_fkey;
ALTER TABLE ONLY public.guild_activity DROP CONSTRAINT guild_activity_activity_id_fkey;
ALTER TABLE ONLY public.youtube_user DROP CONSTRAINT youtube_users_pkey;
ALTER TABLE ONLY public."user" DROP CONSTRAINT user_pkey;
ALTER TABLE ONLY public.guild DROP CONSTRAINT guild_pkey;
ALTER TABLE ONLY public.guild_activity_user DROP CONSTRAINT guild_activity_user_pkey;
ALTER TABLE ONLY public.guild_activity DROP CONSTRAINT guild_activity_pkey;
ALTER TABLE ONLY public.channel DROP CONSTRAINT channel_pkey;
ALTER TABLE ONLY public.activity DROP CONSTRAINT activity_pkey;
ALTER TABLE public.channel ALTER COLUMN id DROP DEFAULT;
ALTER TABLE public.activity ALTER COLUMN id DROP DEFAULT;
DROP TABLE public.youtube_user;
DROP SEQUENCE public.user_id_seq;
DROP TABLE public."user";
DROP SEQUENCE public.guild_id_seq;
DROP SEQUENCE public.guild_guild_activity_next_id_seq;
DROP SEQUENCE public.guild_activity_user_id_seq;
DROP TABLE public.guild_activity_user;
DROP SEQUENCE public.guild_activity_id_within_guild_seq;
DROP SEQUENCE public.guild_activity_guild_id_seq;
DROP SEQUENCE public.guild_activity_activity_id_seq;
DROP TABLE public.guild_activity;
DROP TABLE public.guild;
DROP SEQUENCE public.channel_id_seq;
DROP TABLE public.channel;
DROP SEQUENCE public.activity_id_seq;
DROP SEQUENCE public.activity_application_id_seq;
DROP TABLE public.activity;
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 210 (class 1259 OID 135221)
-- Name: activity; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.activity (
    id bigint NOT NULL,
    name character varying,
    application_id bigint,
    spotify_id character varying,
    album_title character varying,
    track_title character varying,
    state character varying
);


ALTER TABLE public.activity OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 135366)
-- Name: activity_application_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.activity_application_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.activity_application_id_seq OWNER TO postgres;

--
-- TOC entry 3366 (class 0 OID 0)
-- Dependencies: 218
-- Name: activity_application_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.activity_application_id_seq OWNED BY public.activity.application_id;


--
-- TOC entry 209 (class 1259 OID 135220)
-- Name: activity_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.activity_id_seq OWNER TO postgres;

--
-- TOC entry 3367 (class 0 OID 0)
-- Dependencies: 209
-- Name: activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.activity_id_seq OWNED BY public.activity.id;


--
-- TOC entry 225 (class 1259 OID 143574)
-- Name: channel; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.channel (
    id bigint NOT NULL,
    upload_link boolean DEFAULT false NOT NULL
);


ALTER TABLE public.channel OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 143573)
-- Name: channel_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.channel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.channel_id_seq OWNER TO postgres;

--
-- TOC entry 3368 (class 0 OID 0)
-- Dependencies: 224
-- Name: channel_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.channel_id_seq OWNED BY public.channel.id;


--
-- TOC entry 212 (class 1259 OID 135228)
-- Name: guild; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.guild (
    id bigint NOT NULL,
    max_activities integer NOT NULL,
    guild_activity_next_id integer NOT NULL
);


ALTER TABLE public.guild OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 135327)
-- Name: guild_activity; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.guild_activity (
    guild_id bigint NOT NULL,
    activity_id bigint NOT NULL,
    user_id bigint NOT NULL,
    id_within_guild integer NOT NULL,
    action integer NOT NULL,
    countdown_duration integer NOT NULL,
    start_message character varying,
    action_message character varying,
    timeout_duration integer NOT NULL,
    require_whitelist boolean DEFAULT false NOT NULL
);


ALTER TABLE public.guild_activity OWNER TO postgres;

--
-- TOC entry 216 (class 1259 OID 135326)
-- Name: guild_activity_activity_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_activity_activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_activity_activity_id_seq OWNER TO postgres;

--
-- TOC entry 3369 (class 0 OID 0)
-- Dependencies: 216
-- Name: guild_activity_activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_activity_activity_id_seq OWNED BY public.guild_activity.activity_id;


--
-- TOC entry 215 (class 1259 OID 135325)
-- Name: guild_activity_guild_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_activity_guild_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_activity_guild_id_seq OWNER TO postgres;

--
-- TOC entry 3370 (class 0 OID 0)
-- Dependencies: 215
-- Name: guild_activity_guild_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_activity_guild_id_seq OWNED BY public.guild_activity.guild_id;


--
-- TOC entry 221 (class 1259 OID 135401)
-- Name: guild_activity_id_within_guild_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_activity_id_within_guild_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_activity_id_within_guild_seq OWNER TO postgres;

--
-- TOC entry 3371 (class 0 OID 0)
-- Dependencies: 221
-- Name: guild_activity_id_within_guild_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_activity_id_within_guild_seq OWNED BY public.guild_activity.id_within_guild;


--
-- TOC entry 222 (class 1259 OID 135649)
-- Name: guild_activity_user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.guild_activity_user (
    guild_activity_guild_id bigint NOT NULL,
    guild_activity_activity_id bigint NOT NULL,
    user_id bigint NOT NULL,
    whitelisted boolean DEFAULT false NOT NULL,
    blacklisted boolean DEFAULT false NOT NULL
);


ALTER TABLE public.guild_activity_user OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 135395)
-- Name: guild_activity_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_activity_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_activity_user_id_seq OWNER TO postgres;

--
-- TOC entry 3372 (class 0 OID 0)
-- Dependencies: 220
-- Name: guild_activity_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_activity_user_id_seq OWNED BY public.guild_activity.user_id;


--
-- TOC entry 219 (class 1259 OID 135389)
-- Name: guild_guild_activity_next_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_guild_activity_next_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_guild_activity_next_id_seq OWNER TO postgres;

--
-- TOC entry 3373 (class 0 OID 0)
-- Dependencies: 219
-- Name: guild_guild_activity_next_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_guild_activity_next_id_seq OWNED BY public.guild.guild_activity_next_id;


--
-- TOC entry 211 (class 1259 OID 135227)
-- Name: guild_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.guild_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.guild_id_seq OWNER TO postgres;

--
-- TOC entry 3374 (class 0 OID 0)
-- Dependencies: 211
-- Name: guild_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.guild_id_seq OWNED BY public.guild.id;


--
-- TOC entry 214 (class 1259 OID 135235)
-- Name: user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."user" (
    id bigint NOT NULL
);


ALTER TABLE public."user" OWNER TO postgres;

--
-- TOC entry 213 (class 1259 OID 135234)
-- Name: user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_id_seq OWNER TO postgres;

--
-- TOC entry 3375 (class 0 OID 0)
-- Dependencies: 213
-- Name: user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_id_seq OWNED BY public."user".id;


--
-- TOC entry 223 (class 1259 OID 143566)
-- Name: youtube_user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.youtube_user (
    identifier character varying NOT NULL,
    access_token character varying NOT NULL,
    refresh_token character varying NOT NULL,
    expires_in_seconds integer NOT NULL,
    issued timestamp without time zone NOT NULL,
    channel_url character varying
);


ALTER TABLE public.youtube_user OWNER TO postgres;

--
-- TOC entry 3197 (class 2604 OID 135582)
-- Name: activity id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activity ALTER COLUMN id SET DEFAULT nextval('public.activity_id_seq'::regclass);


--
-- TOC entry 3201 (class 2604 OID 143577)
-- Name: channel id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.channel ALTER COLUMN id SET DEFAULT nextval('public.channel_id_seq'::regclass);


--
-- TOC entry 3204 (class 2606 OID 135584)
-- Name: activity activity_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.activity
    ADD CONSTRAINT activity_pkey PRIMARY KEY (id);


--
-- TOC entry 3216 (class 2606 OID 143580)
-- Name: channel channel_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.channel
    ADD CONSTRAINT channel_pkey PRIMARY KEY (id);


--
-- TOC entry 3210 (class 2606 OID 135599)
-- Name: guild_activity guild_activity_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_pkey PRIMARY KEY (guild_id, activity_id);


--
-- TOC entry 3212 (class 2606 OID 135660)
-- Name: guild_activity_user guild_activity_user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_pkey PRIMARY KEY (guild_activity_guild_id, guild_activity_activity_id, user_id);


--
-- TOC entry 3206 (class 2606 OID 135612)
-- Name: guild guild_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild
    ADD CONSTRAINT guild_pkey PRIMARY KEY (id);


--
-- TOC entry 3208 (class 2606 OID 135623)
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (id);


--
-- TOC entry 3214 (class 2606 OID 143572)
-- Name: youtube_user youtube_users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.youtube_user
    ADD CONSTRAINT youtube_users_pkey PRIMARY KEY (identifier);


--
-- TOC entry 3217 (class 2606 OID 135585)
-- Name: guild_activity guild_activity_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_activity_id_fkey FOREIGN KEY (activity_id) REFERENCES public.activity(id);


--
-- TOC entry 3218 (class 2606 OID 135613)
-- Name: guild_activity guild_activity_guild_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_guild_id_fkey FOREIGN KEY (guild_id) REFERENCES public.guild(id);


--
-- TOC entry 3221 (class 2606 OID 135668)
-- Name: guild_activity_user guild_activity_user_guild_activity_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_activity_id_fkey FOREIGN KEY (guild_activity_activity_id) REFERENCES public.activity(id) NOT VALID;


--
-- TOC entry 3220 (class 2606 OID 135663)
-- Name: guild_activity_user guild_activity_user_guild_activity_guild_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_guild_id_fkey FOREIGN KEY (guild_activity_guild_id) REFERENCES public.guild(id) NOT VALID;


--
-- TOC entry 3219 (class 2606 OID 135654)
-- Name: guild_activity_user guild_activity_user_guild_activity_guild_id_guild_activity_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_guild_id_guild_activity_fkey FOREIGN KEY (guild_activity_guild_id, guild_activity_activity_id) REFERENCES public.guild_activity(guild_id, activity_id);


-- Completed on 2022-10-22 23:00:11

--
-- PostgreSQL database dump complete
--

