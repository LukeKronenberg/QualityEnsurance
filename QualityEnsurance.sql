--
-- PostgreSQL database dump
--

-- Dumped from database version 14.2
-- Dumped by pg_dump version 14.4

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

DROP DATABASE quality_ensurance;
--
-- Name: quality_ensurance; Type: DATABASE; Schema: -; Owner: -
--

CREATE DATABASE quality_ensurance WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'English_Switzerland.1252';


\connect quality_ensurance

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

SET default_table_access_method = heap;

--
-- Name: activity; Type: TABLE; Schema: public; Owner: -
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


--
-- Name: activity_application_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.activity_application_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: activity_application_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.activity_application_id_seq OWNED BY public.activity.application_id;


--
-- Name: activity_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.activity_id_seq OWNED BY public.activity.id;


--
-- Name: channel; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.channel (
    id bigint NOT NULL,
    upload_links boolean DEFAULT false NOT NULL
);


--
-- Name: channel_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.channel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: channel_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.channel_id_seq OWNED BY public.channel.id;


--
-- Name: guild; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.guild (
    id bigint NOT NULL,
    max_activities integer NOT NULL,
    guild_activity_next_id integer NOT NULL
);


--
-- Name: guild_activity; Type: TABLE; Schema: public; Owner: -
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


--
-- Name: guild_activity_activity_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_activity_activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_activity_activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_activity_activity_id_seq OWNED BY public.guild_activity.activity_id;


--
-- Name: guild_activity_guild_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_activity_guild_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_activity_guild_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_activity_guild_id_seq OWNED BY public.guild_activity.guild_id;


--
-- Name: guild_activity_id_within_guild_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_activity_id_within_guild_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_activity_id_within_guild_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_activity_id_within_guild_seq OWNED BY public.guild_activity.id_within_guild;


--
-- Name: guild_activity_user; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.guild_activity_user (
    guild_activity_guild_id bigint NOT NULL,
    guild_activity_activity_id bigint NOT NULL,
    user_id bigint NOT NULL,
    whitelisted boolean DEFAULT false NOT NULL,
    blacklisted boolean DEFAULT false NOT NULL
);


--
-- Name: guild_activity_user_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_activity_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_activity_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_activity_user_id_seq OWNED BY public.guild_activity.user_id;


--
-- Name: guild_guild_activity_next_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_guild_activity_next_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_guild_activity_next_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_guild_activity_next_id_seq OWNED BY public.guild.guild_activity_next_id;


--
-- Name: guild_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.guild_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: guild_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.guild_id_seq OWNED BY public.guild.id;


--
-- Name: pending_actions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pending_actions (
    guild_activity_user_guild_id bigint NOT NULL,
    guild_activity_user_activity_id bigint NOT NULL,
    guild_activity_user_user_id bigint NOT NULL,
    start timestamp with time zone NOT NULL,
    eta timestamp with time zone NOT NULL
);


--
-- Name: queued_actions_activity_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.queued_actions_activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: queued_actions_activity_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.queued_actions_activity_id_seq OWNED BY public.pending_actions.guild_activity_user_user_id;


--
-- Name: queued_actions_guild_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.queued_actions_guild_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: queued_actions_guild_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.queued_actions_guild_id_seq OWNED BY public.pending_actions.guild_activity_user_guild_id;


--
-- Name: queued_actions_user_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.queued_actions_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: queued_actions_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.queued_actions_user_id_seq OWNED BY public.pending_actions.guild_activity_user_activity_id;


--
-- Name: user; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."user" (
    id bigint NOT NULL
);


--
-- Name: user_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.user_id_seq OWNED BY public."user".id;


--
-- Name: youtube_user; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.youtube_user (
    identifier character varying NOT NULL,
    access_token character varying NOT NULL,
    refresh_token character varying NOT NULL,
    expires_in_seconds integer NOT NULL,
    issued timestamp without time zone NOT NULL,
    description character varying,
    user_id character varying NOT NULL
);


--
-- Name: activity id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.activity ALTER COLUMN id SET DEFAULT nextval('public.activity_id_seq'::regclass);


--
-- Name: channel id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.channel ALTER COLUMN id SET DEFAULT nextval('public.channel_id_seq'::regclass);


--
-- Name: pending_actions guild_activity_user_guild_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions ALTER COLUMN guild_activity_user_guild_id SET DEFAULT nextval('public.queued_actions_guild_id_seq'::regclass);


--
-- Name: pending_actions guild_activity_user_activity_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions ALTER COLUMN guild_activity_user_activity_id SET DEFAULT nextval('public.queued_actions_user_id_seq'::regclass);


--
-- Name: pending_actions guild_activity_user_user_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions ALTER COLUMN guild_activity_user_user_id SET DEFAULT nextval('public.queued_actions_activity_id_seq'::regclass);


--
-- Name: activity activity_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.activity
    ADD CONSTRAINT activity_pkey PRIMARY KEY (id);


--
-- Name: channel channel_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.channel
    ADD CONSTRAINT channel_pkey PRIMARY KEY (id);


--
-- Name: guild_activity guild_activity_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_pkey PRIMARY KEY (guild_id, activity_id);


--
-- Name: guild_activity_user guild_activity_user_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_pkey PRIMARY KEY (guild_activity_guild_id, guild_activity_activity_id, user_id);


--
-- Name: guild guild_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild
    ADD CONSTRAINT guild_pkey PRIMARY KEY (id);


--
-- Name: pending_actions queued_actions_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_pkey PRIMARY KEY (guild_activity_user_guild_id, guild_activity_user_activity_id, guild_activity_user_user_id);


--
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (id);


--
-- Name: youtube_user youtube_users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.youtube_user
    ADD CONSTRAINT youtube_users_pkey PRIMARY KEY (identifier);


--
-- Name: guild_activity guild_activity_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_activity_id_fkey FOREIGN KEY (activity_id) REFERENCES public.activity(id);


--
-- Name: guild_activity guild_activity_guild_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity
    ADD CONSTRAINT guild_activity_guild_id_fkey FOREIGN KEY (guild_id) REFERENCES public.guild(id);


--
-- Name: guild_activity_user guild_activity_user_guild_activity_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_activity_id_fkey FOREIGN KEY (guild_activity_activity_id) REFERENCES public.activity(id) NOT VALID;


--
-- Name: guild_activity_user guild_activity_user_guild_activity_guild_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_guild_id_fkey FOREIGN KEY (guild_activity_guild_id) REFERENCES public.guild(id) NOT VALID;


--
-- Name: guild_activity_user guild_activity_user_guild_activity_guild_id_guild_activity_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.guild_activity_user
    ADD CONSTRAINT guild_activity_user_guild_activity_guild_id_guild_activity_fkey FOREIGN KEY (guild_activity_guild_id, guild_activity_activity_id) REFERENCES public.guild_activity(guild_id, activity_id);


--
-- Name: pending_actions queued_actions_guild_activity_user_activity_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_guild_activity_user_activity_id_fkey FOREIGN KEY (guild_activity_user_activity_id) REFERENCES public.activity(id) NOT VALID;


--
-- Name: pending_actions queued_actions_guild_activity_user_guild_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_guild_activity_user_guild_id_fkey FOREIGN KEY (guild_activity_user_guild_id) REFERENCES public.guild(id) NOT VALID;


--
-- Name: pending_actions queued_actions_guild_activity_user_guild_id_guild_activit_fkey1; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_guild_activity_user_guild_id_guild_activit_fkey1 FOREIGN KEY (guild_activity_user_guild_id, guild_activity_user_activity_id, guild_activity_user_user_id) REFERENCES public.guild_activity_user(guild_activity_guild_id, guild_activity_activity_id, user_id) NOT VALID;


--
-- Name: pending_actions queued_actions_guild_activity_user_guild_id_guild_activity_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_guild_activity_user_guild_id_guild_activity_fkey FOREIGN KEY (guild_activity_user_guild_id, guild_activity_user_activity_id) REFERENCES public.guild_activity(guild_id, activity_id) NOT VALID;


--
-- Name: pending_actions queued_actions_guild_activity_user_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pending_actions
    ADD CONSTRAINT queued_actions_guild_activity_user_user_id_fkey FOREIGN KEY (guild_activity_user_user_id) REFERENCES public."user"(id) NOT VALID;


--
-- PostgreSQL database dump complete
--

