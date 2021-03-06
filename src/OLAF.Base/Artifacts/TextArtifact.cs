﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OLAF
{
    public class TextArtifact : Artifact
    {
        #region Constructors
        static TextArtifact()
        {
            for(int i = 0; i < SensitiveDataPatternsSource.Count; i++)
            {
                var kv = SensitiveDataPatternsSource.ElementAt(i);
                SensitiveDataPatterns.Add(kv.Key, new Regex(kv.Value, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }
        }
        
        public TextArtifact(string rawText)
        {
            Text = rawText;
            foreach (var p in SensitiveDataPatterns)
            {
                if (p.Value.IsMatch(Text))
                {
                    var match = "";
                    foreach (Match m in p.Value.Matches(Text))
                    {
                        if (m.Success)
                        {
                            Info("Potential sensitive data {0}:{1}.", p.Key, m.Value);
                            match += m.Value + ",";
                        }
                    }
                    SensitiveData.Add(p.Key, match.TrimEnd(','));   
                }    
            }
            if (Pipeline.Dictionaries["competitors_en"].Any(c => Text.ToLower().Contains(c.ToLower())))
            {
                CompetitorNamesPresent.AddRange(Pipeline.Dictionaries["competitors_en"].Where(c => Text.ToLower().Contains(c.ToLower())));
                Info("Text has competitor's name: {0}.", Pipeline.Dictionaries["competitors_en"].First(c => Text.ToLower().Contains(c.ToLower())));
            }


            if (UrlRegEx.IsMatch(Text))
            {
                foreach (Match m in UrlRegEx.Matches(Text))
                {
                    if (m.Success)
                    {
                        Urls.Add(m.Value);
                    }
                }
            }

            Debug("Added text artifact with {0} characters.", Text.Length);
        }

        public TextArtifact(string name, string rawText) : this(rawText)
        {
            Name = name;
        }
        #endregion

        #region Properties
        public List<string> RawText { get; }

        public string Text { get; protected set; }
        
        public ImageArtifact Image { get; set; }

        public Dictionary<string, double> Sentiment { get; protected set; }

        public List<string> Urls { get; } = new List<string>();

        public List<string> Entities { get; } = new List<string>();

        public List<string> KeyWords { get; } = new List<string>();

        public List<string> CompetitorNamesPresent { get; protected set; } = new List<string>();

        public Dictionary<string, string> SensitiveData { get; protected set; } = new Dictionary<string, string>();

        public bool HasSensitiveData => SensitiveData.Count > 0;
        
        public Dictionary<string, string> Languages { get; protected set; } = new Dictionary<string, string>();

        public Dictionary<string, object> Metadata { get; protected set; } = new Dictionary<string, object>();

        public static Regex UrlRegEx { get; } = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Dictionary<string, string> SensitiveDataPatternsSource { get; protected set; } = new Dictionary<string, string>()
        {
            { "Credit Card", @"\d{4}[ -]?\d{4}[ -]?\d{4}[ -]?\d{4}|\d{4}[ -]?\d{6}[ -]?\d{4}\d?/" },
            { "SSN", @"\b\d{3}[ -.]\d{2}[ -.]\d{4}\b" },
            { "credentials", @"(login( cred(ential)?s| info(rmation)?)?|cred(ential)?s) ?:\s*\S+\s+\/?\s*\S+" },
            { "emailaddress", @"([a-z0-9_\-.+]+)@\w+(\.\w+)*" },
            { "username", @"(user( ?name)?|login): \S+" },
            { "password", @"(pass(word|phrase)?|secret): \S+" },
            { "secret", @"\.*(secret|confidential|classified)\.*" }
        };

        public static Dictionary<string, Regex> SensitiveDataPatterns { get; } = new Dictionary<string, Regex>();
        #endregion

        #region Methods
        public static string GetAlphaNumericString(string s)
        {
            StringBuilder an = new StringBuilder(s.Length);
            char p = ' ';
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c))
                {
                    an.Append(c);
                    p = c;
                }
                /*
                else if (char.IsPunctuation(c) && char.IsLetterOrDigit(p))
                {
                    an.Append(c);
                    p = c;
                }*/
                else if (char.IsWhiteSpace(c) && (char.IsLetterOrDigit(p) || char.IsPunctuation(p)))
                {
                    an.Append(c);
                    p = c;
                }
                else continue;

            }
            return an.ToString();
        }

        public static bool IsNumber(string s) => s.All(c => Char.IsDigit(c));
        #endregion

        #region Fields

        #region Dictionaries

        #region Slang
        protected static string[] slangWords = @"121|	one to one
            a/s/l|	age, sex, location
            adn|	any day now
            afaik|	as far as I know
            afk|	away from keyboard
            aight|	alright
            alol|	actually laughing out loud
            b4|	before
            b4n|	bye for now
            bak|	back at the keyboard
            bf|	boyfriend
            bff|	best friends forever
            bfn|	bye for now
            bg|	big grin
            bta|	but then again
            btw|	by the way
            cid|	crying in disgrace
            cnp|	continued in my next post
            cp|	chat post
            cu|	see you
            cul|	see you later
            cul8r|	see you later 
            cya|	bye
            cyo|	see you online 
            dbau|	doing business as usual 
            fud|	fear, uncertainty, and doubt 
            fwiw|	for what it's worth 
            fyi|	for your information
            g|	grin 
            g2g|	got to go 
            ga|	go ahead 
            gal|	get a life 
            gf|	girlfriend 
            gfn|	gone for now
            gmbo|	giggling my butt off 
            gmta|	great minds think alike 
            h8|	hate
            hagn|	have a good night 
            hdop|	help delete online predators 
            hhis|	hanging head in shame 
            iac|	in any case 
            ianal|	I am not a lawyer
            ic|	I see 
            idk|	I don't know 
            imao|	in my arrogant opinion
            imnsho|	in my not so humble opinion 
            imo|	in my opinion 
            iow|	in other words 
            ipn|	I’m posting naked 
            irl|	in real life 
            jk|	just kidding
            l8r|	later
            ld|	later, dude 
            ldr|	long distance relationship 
            llta|	lots and lots of thunderous applause 
            lmao|	laugh my ass off
            lmirl	let's meet in real life 
            lol|	laugh out loud
            ltr|	longterm relationship 
            lulab|	love you like a brother 
            lulas|	love you like a sister 
            luv|	love
            m/f|	male or female 
            m8|	mate
            milf|	mother I would like to fuck
            oll|	online love 
            omg|	oh my god
            otoh|	on the other hand 
            pir|	parent in room
            ppl|	people
            r|	are
            rofl|	roll on the floor laughing
            rpg|	role playing games
            ru|	are you
            shid|	slaps head in disgust
            somy|	sick of me yet
            sot|	short of time 
            thanx|	thanks
            thx|	thanks
            ttyl|	talk to you later 
            u|	you
            ur|	you are
            uw|	you’re welcome 
            wb|	welcome back 
            wfm|	works for me 
            wibni|	wouldn't it be nice if 
            wtf|	what the fuck
            wtg|	way to go
            wtgp|	want to go private
            ym|	young man".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #region Hedge
        protected static string[] hedgeWords = {"almost", "apparent", "apparently", "appear", "appeared", "appears",
          "approximately", "argue", "argued", "argues", "around", "assume",
          "assumed", "broadly", "certain amount", "certain extent",
          "certain level", "claim", "claimed", "claims", "doubt", "doubtful",
          "essentially", "estimate", "estimated", "fairly", "feel", "feels",
          "felt", "frequently", "from my perspective", "from our perspective",
          "from this perspective", "generally", "guess", "in general",
          "in most cases", "in most instances", "in my opinion", "in my view",
          "in our opinion", "in our view", "in this view", "indicate",
          "indicated", "indicates", "largely", "likely", "mainly", "may",
          "maybe", "might", "mostly", "often", "on the whole", "ought",
          "perhaps", "plausible", "plausibly", "possible", "possibly",
          "postulate", "postulated", "postulates", "presumable", "presumably",
          "probable", "probably", "quite", "rather", "relatively", "roughly",
          "seems", "should", "sometimes", "somewhat", "suggest", "suggested",
          "suggests", "suppose", "supposed", "supposes", "suspect", "suspects",
          "tend to", "tended to", "tends to", "think", "thinking", "thought",
          "to my knowledge", "typical", "typically", "uncertain", "uncertainly",
          "unclear", "unclearly", "unlikely", "usually" };
        #endregion

        #region Profanity
        protected static string[] profanityWords = { "damn", "dyke", "fuck", "shit", "ahole", "amcik", "andskota", "anus",
            "arschloch", "arse", "ash0le", "ash0les", "asholes", "ass", "Ass Monkey", "Assface",
            "assh0le", "assh0lez", "asshole", "assholes", "assholz", "assrammer", "asswipe", "ayir",
            "azzhole", "b00b", "b00bs", "b17ch", "b1tch", "bassterds", "bastard",
            "bastards", "bastardz", "basterds", "basterdz", "bi7ch", "Biatch", "bitch", "bitch",
            "bitches", "Blow Job", "blowjob", "boffing", "boiolas", "bollock", "boobs", "breasts",
            "buceta", "butt-pirate", "butthole", "buttwipe", "c0ck", "c0cks",
            "c0k", "cabron", "Carpet Muncher", "cawk", "cawks", "cazzo", "chink", "chraa", "chuj",
            "cipa", "clit", "Clit", "clits", "cnts", "cntz", "cock", "cock-head", "cock-sucker",
            "Cock", "cockhead", "cocks", "CockSucker", "crap", "cum", "cunt",
            "cunt", "cunts", "cuntz", "d4mn", "daygo", "dego", "dick", "dick", "dike", "dild0",
            "dild0s", "dildo", "dildos", "dilld0", "dilld0s", "dirsa", "dominatricks", "dominatrics",
            "dominatrix", "dupa", "dyke", "dziwka", "ejackulate", "ejakulate", "Ekrem", "Ekto", "enculer",
            "enema", "f u c k", "f u c k e r", "faen", "fag", "fag", "fag1t", "faget",
            "fagg1t", "faggit", "faggot", "fagit", "fags", "fagz", "faig", "faigs", "fanculo", "fanny",
            "fart", "fatass", "fcuk", "feces", "feg", "Felcher", "ficken", "fitt", "Flikker", "flipping the bird",
            "foreskin", "Fotze", "fuck", "fucker", "fuckin", "fucking", "fucks", "Fudge Packer", "fuk", "fuk",
            "Fukah", "Fuken", "fuker", "Fukin", "Fukk", "Fukkah", "Fukker", "Fukkin", "futkretzn", "fux0r",
            "g00k", "gay", "gayboy", "gaygirl", "gays", "gayz", "God-damned", "gook",
            "guiena", "h00r", "h0ar", "h0r", "h0re", "h4x0r", "hell", "hells", "helvete", "hoar", "hoer",
            "hoer", "honkey", "hoore", "hore", "Huevon", "hui", "injun", "jackoff", "jap", "japs", "jerk-off",
            "jisim", "jism", "jiss", "jizm", "jizz", "kanker", "kawk", "kike", "klootzak", "knob", "knobs",
            "knobz", "knulle", "kraut", "kuk", "kuksuger", "kunt", "kunts", "kuntz", "Kurac", "kurwa", "kusi",
            "kyrpa", "l3i+ch", "l3itch", "lesbian", "Lesbian", "lesbo", "Lezzian",
            "Lipshitz", "mamhoon", "masochist", "masokist", "massterbait", "masstrbait", "masstrbate",
            "masterbaiter", "masterbat", "masterbat3", "masterbate", "masterbates", "masturbat", "masturbate",
            "merd", "mibun", "mofo", "monkleigh", "Motha Fucker", "Motha Fuker", "Motha Fukkah", "Motha Fukker",
            "mother-fucker", "Mother Fucker", "Mother Fukah", "Mother Fuker", "Mother Fukker", "motherfucker",
            "mouliewop", "muie", "mulkku", "muschi", "Mutha Fucker", "Mutha Fukah", "Mutha Fuker",
            "Mutha Fukkah", "Mutha Fukker", "n1gr", "nastt", "nazi", "nazis", "nepesaurio", "nigga", "nigger",
            "nigger", "nigger;", "nigur;", "niiger;", "niigr;", "nutsack", "orafis", "orgasim;", "orgasm", "orgasum",
            "oriface", "orifice", "orifiss", "orospu", "p0rn", "packi", "packie", "packy", "paki", "pakie", "paky",
            "paska", "pecker", "peeenus", "peeenusss", "peenus", "peinus", "pen1s", "penas", "penis", "penis-breath",
            "penus", "penuus", "perse", "Phuc", "phuck", "Phuck",  "Phuker", "Phukker", "picka", "pierdol", "pillu",
            "pimmel", "pimpis", "piss", "pizda", "polac", "polack", "polak",  "poontsee", "poop", "porn", "pr0n",
            "pr1c", "pr1ck", "pr1k", "preteen", "pula", "pule", "pusse", "pussee", "pussy", "puto", "puuke", "puuker",
            "qahbeh", "queef", "queer", "queers", "queerz", "qweers", "qweerz", "qweir", "rautenberg",
            "rectum", "retard", "sadist", "scank", "schaffer", "scheiss", "schlampe", "schlong", "schmuck", "screw",
            "screwing", "scrotum", "semen", "sex", "sexy", "sh!t", "Sh!t", "sh!t", "sh1t", "sh1ter", "sh1ts", "sh1tter",
            "sh1tz", "sharmuta", "shemale", "shi+", "shipal", "shit", "shits", "shitter", "Shitty", "Shity", "shitz",
            "shiz", "Shyt", "Shyte", "Shytty",  "skanck", "skank", "skankee", "skankey", "skanks", "Skanky", "skribz",
            "skurwysyn", "slut", "sluts", "Slutty", "slutz", "son-of-a-bitch", "sphencter", "spic", "spierdalaj",
            "splooge", "suka", "teets", "teez", "testical", "testicle", "testicle", "tit", "tits", "titt", "titt",
            "turd", "twat", "va1jina", "vag1na", "vagiina", "vagina", "vaj1na", "vajina", "vittu",
            "vulva", "w00se", "w0p", "wank", "wank", "wetback", "wh00r", "wh0re", "whoar", "whore",
            "wichser", "wop", "xrated", "xxx", "Lipshits", "Mother Fukkah", "zabourah", "Phuk", "Poonani",
            "puta", "recktum", "sharmute", "Shyty", "smut", "vullva", "yed"};
        #endregion

        #region Negative Emotion
        protected static string[] negativeEmotionWordLabels = "negative-fear;sadness;general-dislike;ingratitude;shame;compassion;humility;despair;anxiety;daze".Split(';');

        protected static string[] negativeEmotionWords = @"affright;aggrieve;abhor;;abase;affectionate;abase;abject;afraid;bedaze
                afraid;bad;abhorrent;;abash;caring;chagrin;abjectly;anxious;daze
                alarm;bereaved;abominably;;abashed;commiserate;demeaning;baffled;anxiously;dazed
                alarmed;bereft;abominate;;ashamed;compassionate;demeaningly;balked;apprehensive;dazzling
                alert;blue;afraid;;awkward;condole_with;embarrassed;cynical;apprehensively;dazzlingly
                anxious;bored;aggravate;;black;excusable;humble;defeat;brood;fulgurant
                anxiously;cast_down;aggravated;;broken;feel_for;humbling;defeated;concern;fulgurous
                appal;cheerless;aggressive;;chagrin;fond;humiliate;demoralized;concerned;stun
                appall;cheerlessly;alien;;chagrined;forgivable;humiliated;despair;discomfit;stunned
                apprehensive;contrite;alienate;;confuse;forgive;humiliating;despairing;discomfited;stupefied
                apprehensively;contritely;alienated;;confused;lovesome;humiliatingly;despairingly;discompose;stupid
                atrocious;dark;amok;;confusedly;merciful;mortified;desperate;disconcert;
                awful;deject;amuck;;confusing;mercifully;mortify;despondently;disquieted;
                awfully;demoralising;anger;;consternate;pity;mortifying;disappointed;distress;
                bashfully;demoralize;angered;;discomfit;showing_mercy;self-deprecating;discomfited;distressed;
                browbeaten;demoralized;angrily;;discomfited;sympathize;;discourage;distressful;
                bullied;demoralizing;angry;;discompose;sympathize_with;;discouraged;distressfully;
                chill;deplorable;annoy;;disconcert;tender;;discouraging;distressing;
                chilling;deplorably;annoyed;;discreditably;tenderly;;disheartened;distressingly;
                cliff-hanging;depress;annoying;;discredited;venial;;dispiritedly;disturbed;
                cowed;depressed;anomic;;disgraced;warm;;foiled;disturbing;
                cower;depressing;avaricious;;disgraceful;with_mercy;;frustrated;dwell;
                crawl;depressive;baffled;;disgracefully;;;hopeless;dysphoric;
                creep;desolate;balked;;dishonorably;;;hopelessly;edgy;
                cringe;despairingly;bedevil;;dishonored;;;misanthropic;embarrassed;
                cruel;despondent;begrudge;;dishonourably;;;misanthropical;fidgety;
                cruelly;despondently;begrudging;;disordered;;;overcome;fretful;
                dash;dingy;belligerent;;embarrass;;;pessimistic;high-strung;
                daunt;disconsolate;belligerently;;embarrassed;;;pessimistically;impatient;
                diffident;discouraged;bother;;embarrassing;;;reconcile;impatiently;
                diffidently;disheartened;bothersome;;embarrassingly;;;resign;in_suspense;
                dire;disheartening;brood;;flurry;;;resigned;insecure;
                direful;dismal;choleric;;guilty;;;resignedly;insecurely;
                dismay;dismay;churn_up;;hangdog;;;submit;interest;
                dread;dispirit;contemn;;humble;;;thwarted;itchy;
                dreaded;dispirited;covet;;humiliate;;;unhopeful;jittery;
                dreadful;dispiriting;covetous;;humiliated;;;;jumpy;
                dreadfully;distressed;covetously;;ignominious;;;;nervous;
                fawn;doleful;crucify;;ignominiously;;;;nervy;
                fear;dolefully;despise;;inglorious;;;;occupy;
                fearful;dolorous;despiteful;;ingloriously;;;;overstrung;
                fearfully;dolourous;detached;;mortified;;;;painfully;
                fearsome;down;detest;;mortify;;;;perturbing;
                fright;downcast;detestable;;mortifying;;;;raring;
                frighten;downhearted;detestably;;opprobrious;;;;restive;
                frighten_away;downtrodden;devil;;put_off;;;;restless;
                frighten_off;drab;disaffect;;scandalous;;;;solicitous;
                frightened;drear;disapprove;;self-conscious;;;;solicitously;
                frightening;dreary;disapproving;;self-consciously;;;;troubling;
                frighteningly;dysphoric;discouraged;;shame;;;;uneasily;
                frightful;execrable;discouraging;;shamed;;;;uneasy;
                grovel;forlorn;disdain;;shamefaced;;;;unhappy;
                hangdog;forlornly;disgust;;shamefacedly;;;;unquiet;
                hardhearted;get_down;disgusted;;shameful;;;;untune;
                heartless;gloomful;disgustedly;;shamefully;;;;upset;
                heartlessly;gloomily;disgustful;;sheepish;;;;uptight;
                hesitantly;glooming;disgusting;;shocking;;;;with_impatience;
                hesitatingly;gloomy;disgustingly;;sticky;;;;worried;
                hideous;glum;disincline;;unenviable;;;;worrisome;
                hideously;godforsaken;disinclined;;untune;;;;worry;
                horrendous;grief-stricken;dislikable;;upset;;;;worrying;
                horrible;grieve;dislike;;;;;;worryingly;
                horribly;grieving;disoriented;;;;;;;
                horrid;grievous;displease;;;;;;;
                horridly;grievously;displeased;;;;;;;
                horrific;grim;displeasing;;;;;;;
                horrified;guilty;displeasingly;;;;;;;
                horrify;hangdog;distasteful;;;;;;;
                horrifying;hapless;distastefully;;;;;;;
                horrifyingly;harass;dun;;;;;;;
                horror-stricken;heartbreaking;enfuriate;;;;;;;
                horror-struck;heartrending;enraged;;;;;;;
                hysterical;heartsick;enviable;;;;;;;
                hysterically;heavyhearted;enviably;;;;;;;
                intimidate;joyless;envious;;;;;;;
                intimidated;joylessly;enviously;;;;;;;
                monstrously;lachrymose;envy;;;;;;;
                outrageous;laden;estrange;;;;;;;
                pall;lamentably;estranged;;;;;;;
                panic;long-faced;evil;;;;;;;
                panicked;lorn;exacerbate;;;;;;;
                panicky;low;exasperate;;;;;;;
                panic-stricken;low-spirited;exasperating;;;;;;;
                panic-struck;melancholic;execrate;;;;;;;
                scare;melancholy;fed_up;;;;;;;
                scare_away;miserable;foul;;;;;;;
                scare_off;miserably;frustrate;;;;;;;
                scared;misfortunate;frustrated;;;;;;;
                scarey;mournful;frustrating;;;;;;;
                scarily;mournfully;furious;;;;;;;
                scary;mourning;furiously;;;;;;;
                shivery;oppress;galling;;;;;;;
                shuddery;oppressed;get_at;;;;;;;
                shy;oppressive;get_to;;;;;;;
                shyly;oppressively;grabby;;;;;;;
                suspenseful;pathetic;grasping;;;;;;;
                suspensive;penitent;gravel;;;;;;;
                terrible;penitentially;greedy;;;;;;;
                terrified;penitently;green-eyed;;;;;;;
                timid;persecute;grizzle;;;;;;;
                timidly;persecuted;grudge;;;;;;;
                timorous;piteous;grudging;;;;;;;
                timorously;pitiable;harass;;;;;;;
                trepid;pitiful;harassed;;;;;;;
                trepidly;pitying;harried;;;;;;;
                ugly;plaintive;hate;;;;;;;
                unassertive;plaintively;hateful;;;;;;;
                unassertively;poor;hatefully;;;;;;;
                uneasily;regret;hideous;;;;;;;
                unkind;regretful;horrid;;;;;;;
                unsure;remorseful;horrific;;;;;;;
                ;remorsefully;hostile;;;;;;;
                ;repent;hostilely;;;;;;;
                ;repentant;huffily;;;;;;;
                ;repentantly;huffish;;;;;;;
                ;rue;huffy;;;;;;;
                ;rueful;incense;;;;;;;
                ;ruefully;incensed;;;;;;;
                ;sad;indignant;;;;;;;
                ;sadden;indignantly;;;;;;;
                ;saddening;indispose;;;;;;;
                ;sadly;infuriate;;;;;;;
                ;shamed;infuriated;;;;;;;
                ;shamefaced;infuriating;;;;;;;
                ;sorrow;inimical;;;;;;;
                ;sorrowful;irascible;;;;;;;
                ;sorrowfully;irritate;;;;;;;
                ;sorrowing;irritated;;;;;;;
                ;sorry;irritating;;;;;;;
                ;sorry_for;isolated;;;;;;;
                ;suffering;jealous;;;;;;;
                ;tearful;jealously;;;;;;;
                ;tyrannical;livid;;;;;;;
                ;tyrannous;lividly;;;;;;;
                ;uncheerful;loathe;;;;;;;
                ;unhappy;loathly;;;;;;;
                ;weeping;loathsome;;;;;;;
                ;woebegone;mad;;;;;;;
                ;woeful;maddened;;;;;;;
                ;woefully;maddening;;;;;;;
                ;world-weary;malefic;;;;;;;
                ;wretched;malevolent;;;;;;;
                ;;malevolently;;;;;;;
                ;;malicious;;;;;;;
                ;;maliciously;;;;;;;
                ;;malign;;;;;;;
                ;;misanthropic;;;;;;;
                ;;misanthropical;;;;;;;
                ;;misogynic;;;;;;;
                ;;murderously;;;;;;;
                ;;nark;;;;;;;
                ;;nauseate;;;;;;;
                ;;nauseated;;;;;;;
                ;;nauseating;;;;;;;
                ;;nauseous;;;;;;;
                ;;nettle;;;;;;;
                ;;nettled;;;;;;;
                ;;nettlesome;;;;;;;
                ;;noisome;;;;;;;
                ;;obscene;;;;;;;
                ;;odiously;;;;;;;
                ;;offend;;;;;;;
                ;;offensive;;;;;;;
                ;;oppress;;;;;;;
                ;;outrage;;;;;;;
                ;;outraged;;;;;;;
                ;;outrageous;;;;;;;
                ;;overjealous;;;;;;;
                ;;peeved;;;;;;;
                ;;persecute;;;;;;;
                ;;pesky;;;;;;;
                ;;pestered;;;;;;;
                ;;pestering;;;;;;;
                ;;pestiferous;;;;;;;
                ;;pique;;;;;;;
                ;;pissed;;;;;;;
                ;;plaguey;;;;;;;
                ;;plaguy;;;;;;;
                ;;pout;;;;;;;
                ;;prehensile;;;;;;;
                ;;provoked;;;;;;;
                ;;queasy;;;;;;;
                ;;rag;;;;;;;
                ;;reject;;;;;;;
                ;;repel;;;;;;;
                ;;repellant;;;;;;;
                ;;repellent;;;;;;;
                ;;repugnant;;;;;;;
                ;;repulse;;;;;;;
                ;;repulsive;;;;;;;
                ;;repulsively;;;;;;;
                ;;resentful;;;;;;;
                ;;resentfully;;;;;;;
                ;;revengefully;;;;;;;
                ;;revolt;;;;;;;
                ;;revolting;;;;;;;
                ;;revoltingly;;;;;;;
                ;;rile;;;;;;;
                ;;riled;;;;;;;
                ;;roiled;;;;;;;
                ;;scorn;;;;;;;
                ;;see_red;;;;;;;
                ;;separated;;;;;;;
                ;;set-apart;;;;;;;
                ;;sick;;;;;;;
                ;;sick_of;;;;;;;
                ;;sicken;;;;;;;
                ;;sickening;;;;;;;
                ;;sickeningly;;;;;;;
                ;;sickish;;;;;;;
                ;;sore;;;;;;;
                ;;spiteful;;;;;;;
                ;;stew;;;;;;;
                ;;stung;;;;;;;
                ;;sulk;;;;;;;
                ;;sulky;;;;;;;
                ;;tantalize;;;;;;;
                ;;teasing;;;;;;;
                ;;tired_of;;;;;;;
                ;;torment;;;;;;;
                ;;turn_off;;;;;;;
                ;;umbrageous;;;;;;;
                ;;unfriendly;;;;;;;
                ;;vengefully;;;;;;;
                ;;vex;;;;;;;
                ;;vexatious;;;;;;;
                ;;vexed;;;;;;;
                ;;vexing;;;;;;;
                ;;vile;;;;;;;
                ;;vindictive;;;;;;;
                ;;vindictively;;;;;;;
                ;;wicked;;;;;;;
                ;;with_hostility;;;;;;;
                ;;wrathful;;;;;;;
                ;;wrathfully;;;;;;;
                ;;wroth;;;;;;;
                ;;wrothful;;;;;;;
                ;;yucky;;;;;;;".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #region Positive Emotion
        protected static string[] positiveEmotionWordLabels = "joy;love;enthusiasm;gratitude;self-pride;calmness;fearlessness;positive-expectation;positive-hope;positive-fear;affection;liking".Split(';');

        protected static string[] positiveEmotionWords = @"appreciated;admirable;avid;appreciatively;;allay;assure;anticipate;bucked_up;;;
            banter;admirably;eager;grateful;;assuasive;confident;cliff-hanging;encourage;;;
            barrack;admire;eagerly;gratefully;;at_ease;convinced;look_for;encouraged;;;
            be_on_cloud_nine;adorably;ebulliently;thankful;;calm;dauntlessly;look_to;encouraging;;;
            beaming;adoring;enthusiastic;;;calm_down;doughty;suspenseful;encouragingly;;;
            blithely;affect;enthusiastically;;;calming;fearless;suspensive;hope;;;
            carefree;affectional;exciting;;;calmly;fearlessly;;hopeful;;;
            chaff;affectionate;expansively;;;chill;hardy;;hopefully;;;
            cheer;affective;exuberantly;;;cold;intrepidly;;optimistic;;;
            cheer_up;amative;great;;;cool;positive;;optimistically;;;
            cheerful;amatory;riotously;;;cool_down;reassure;;sanguine;;;
            cheerfully;amicable;thirstily;;;dreamy;reassured;;;;;
            cheering;amicably;zealous;;;ease;reassuring;;;;;
            chirk_up;amorous;;;;easily;reassuringly;;;;;
            close;approbative;;;;easy;unafraid;;;;;
            comfort;approbatory;;;;lackadaisical;;;;;;
            comfortable;approve;;;;languid;;;;;;
            comfortably;approved;;;;languorous;;;;;;
            comforting;approving;;;;languorously;;;;;;
            complacent;becharm;;;;lull;;;;;;
            congratulate;beguile;;;;lulling;;;;;;
            console;beguiled;;;;pacifically;;;;;;
            content;benefic;;;;pacifying;;;;;;
            contented;beneficed;;;;peaceable;;;;;;
            ebulliently;beneficent;;;;peaceably;;;;;;
            elate;beneficially;;;;peace-loving;;;;;;
            elated;benevolent;;;;placid;;;;;;
            elating;benevolently;;;;placidly;;;;;;
            embolden;bewitch;;;;quiet;;;;;;
            euphoriant;bewitching;;;;quieten;;;;;;
            euphoric;brotherlike;;;;quietening;;;;;;
            exalt;brotherly;;;;relieve;;;;;;
            exhilarate;captivate;;;;serene;;;;;;
            exhilarated;captivated;;;;soothing;;;;;;
            exhilarating;captivating;;;;still;;;;;;
            exhort;capture;;;;tranquil;;;;;;
            expansively;caring;;;;tranquilize;;;;;;
            exuberantly;caring;;;;tranquillize;;;;;;
            exult;catch;;;;tranquilly;;;;;;
            exultant;charm;;;;unagitated;;;;;;
            exultantly;charmed;;;;unruffled;;;;;;
            exulting;commendable;;;;;;;;;;
            exultingly;delighted;;;;;;;;;;
            fulfil;devoted;;;;;;;;;;
            fulfill;emotive;;;;;;;;;;
            gay;enamor;;;;;;;;;;
            gayly;enamour;;;;;;;;;;
            glad;enchant;;;;;;;;;;
            gladden;enchanting;;;;;;;;;;
            gladdened;endearingly;;;;;;;;;;
            gladsome;enjoy;;;;;;;;;;
            gleeful;enthralled;;;;;;;;;;
            gleefully;enthralling;;;;;;;;;;
            gloatingly;entrance;;;;;;;;;;
            gratify;entranced;;;;;;;;;;
            gratifying;entrancing;;;;;;;;;;
            gratifyingly;fascinate;;;;;;;;;;
            happily;fascinating;;;;;;;;;;
            happy;favor;;hero;;;;;;;;
            hearten;favorable;;;;;;;;;;
            hilarious;favorably;;;;;;;;;;
            hilariously;favour;;;;;;;;;;
            inspire;favourable;;;;;;;;;;
            intoxicate;favourably;;;;;;;;;;
            jocund;fond;;;;;;;;;;
            jolly;fondly;;;;;;;;;;
            jolly_along;fraternal;;;;;;;;;;
            jolly_up;friendly;;;;;;;;;;
            jovial;giving_protection;;;;;;;;;;
            joy;good;;;;;;;;;;
            joyful;impress;;;;;;;;;;
            joyfully;laudably;;;;;;;;;;
            joyous;likable;;;;;;;;;;
            joyously;like;;;;;;;;;;
            jubilant;likeable;;;;;;;;;;
            jubilantly;look_up_to;;;;;;;;;;
            jubilate;love;;;;;;;;;;
            jump_for_joy;lovesome;;;;;;;;;;
            kid;loving;;;;;;;;;;
            lift_up;lovingly;;;;;;;;;;
            live_up_to;move;;;;;;;;;;
            merrily;offering_protection;;;;;;;;;;
            merry;praiseworthily;;;;;;;;;;
            mirthful;protective;;;;;;;;;;
            mirthfully;protectively;;;;;;;;;;
            near;romantic;;;;;;;;;;
            nigh;strike;;;;;;;;;;
            pep_up;tender;;;;;;;;;;
            pick_up;trance;;;;;;;;;;
            pleased;warm;;;;;;;;;;
            pleasing;warmhearted;;;;;;;;;;
            preen;worshipful;;;;;;;;;;
            pride;;;;;;;;;;;
            prideful;;;;;;;;;;;
            proudly;;;;;;;;;;;
            recreate;;;;;;;;;;;
            rejoice;;;;;;;;;;;
            rejoicing;;;;;;;;;;;
            revel;;;;;;;;;;;
            riotously;;;;;;;;;;;
            satiable;;;;;;;;;;;
            satisfactorily;;;;;;;;;;;
            satisfactory;;;;;;;;;;;
            satisfiable;;;;;;;;;;;
            satisfied;;;;;;;;;;;
            satisfy;;;;;;;;;;;
            satisfying;;;;;;;;;;;
            satisfyingly;;;;;;;;;;;
            screaming;;;;;;;;;;;
            self-satisfied;;;;;;;;;;;
            smug;;;;;;;;;;;
            solace;;;;;;;;;;;
            soothe;;;;;;;;;;;
            stimulating;;;;;;;;;;;
            teased;;;thanks;;;;;;;;
            thrill;;;;;;;;;;;
            tickle;;;;;;;;;;;
            titillate;;;;;;;;;;;
            titillated;;;;;;;;;;;
            titillating;;;;;;;;;;;
            triumph;;;;;;;;;;;
            triumphal;;;;;;;;;;;
            triumphant;;;;;;;;;;;
            triumphantly;;;;;;;;;;;
            unworried;;;;;;;;;;;
            uplift;;;;;;;;;;;
            uproarious;;;;;;;;;;;
            uproariously;;;;;;;;;;;
            urge;;;;;;;;;;;
            urge_on;;;;;;;;;;;
            walk_on_air;;;;;;;;;;;
            wallow;;;;;;;;;;;
            with_happiness;;;;;;;;;;;
            with_pride;;;;;;;;;;;".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #region Ambiguous Emotion
        protected static string[] ambiguousEmotionWordLabels = "thing; gravity;surprise;ambiguous-agitation;ambiguous-fear;pensiveness;ambiguous-expectation".Split(';');

        protected static string[] ambiguousEmotionWords = @";dear;amaze;agitate;fear;brooding;anticipant
            ;devout;amazed;agitated;hero-worship;broody;anticipate
            ;earnest;amazing;electrifying;idolize;contemplative;anticipative
            ;earnestly;amazingly;excite;revere;meditative;desire
            ;heartfelt;astonied;fire_up;reverence;musing;expect
            ;in_earnest;astonish;foment;reverent;pensive;expectant
            ;seriously;astonished;heat;reverentially;pensively;expectantly
            ;;astonishing;ignite;reverently;pondering;fevered
            ;;astonishingly;incite;venerate;reflective;feverish
            ;;astound;inflame;worship;ruminative;feverishly
            ;;astounded;instigate;;wistful;hectic
            ;;astounding;scandalmongering;;;hope
            ;;awe;sensational;;;hopeful
            ;;awed;sensationalistic;;;hopefully
            ;;awestricken;sensationally;;;trust
            ;;awestruck;set_off;;;
            ;;awful;shake;;;
            ;;baffle;shake_up;;;
            ;;beat;stimulate;;;
            ;;besot;stir;;;
            ;;bewilder;stir_up;;;
            ;;dazed;thrilling;;;
            ;;dumbfound;touch;;;
            ;;dumbfounded;yellow;;;
            ;;dumfounded;;;;
            ;;fantastic;;;;
            ;;flabbergasted;;;;
            ;;flummox;;;;
            ;;get;;;;
            ;;gravel;;;;
            ;;howling;;;;
            ;;in_awe_of;;;;
            ;;marvel;;;;
            ;;marvellously;;;;
            ;;marvelous;;;;
            ;;marvelously;;;;
            ;;mystify;;;;
            ;;nonplus;;;;
            ;;perplex;;;;
            ;;puzzle;;;;
            ;;rattling;;;;
            ;;staggering;;;;
            ;;stun;;;;
            ;;stunned;;;;
            ;;stupefied;;;;
            ;;stupefy;;;;
            ;;stupefying;;;;
            ;;stupid;;;;
            ;;stupify;;;;
            ;;superbly;;;;
            ;;surprise;;;;
            ;;surprised;;;;
            ;;surprisedly;;;;
            ;;surprising;;;;
            ;;surprisingly;;;;
            ;;terrific;;;;
            ;;terrifically;;;;
            ;;thunderstruck;;;;
            ;;toppingly;;;;
            ;;tremendous;;;;
            ;;trounce;;;;
            ;;wonder;;;;
            ;;wonderful;;;;
            ;;wonderfully;;;;
            ;;wondrous;;;;
            ;;wondrously;;;;".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #region Emoticon
        protected static string[] emoticonWords = @"%-(	-1
            %-)	1
            (-:	1
            (:	1
            (^ ^)	1
            (^-^)	1
            (^.^)	1
            (^_^)	1
            (o:	1
            (o;	0
            )-:	-1
            ):	-1
            )o:	-1
            *)	0
            *\o/*	1
            --^--@	1
            0:)	1
            38*	-1
            8)	1
            8-)	0
            8-0	-1
            8/	-1
            8\	-1
            8c	-1
            :#	-1
            :'(	-1
            :'-(	-1
            :(	-1
            :)	1
            :*(	-1
            :,(	-1
            :-&	-1
            :-(	-1
            :-(o)	-1
            :-)	1
            :-*	1
            :-*	1
            :-/	-1
            :-/	0
            :-D	1
            :-O	0
            :-P	1
            :-S	-1
            :-\	-1
            :-\	0
            :-|	-1
            :-}	1
            :/	-1
            :0->-<|:	0
            :3	1
            :9	1
            :D	1
            :E	-1
            :F	-1
            :O	-1
            :P	1
            :P	1
            :S	-1
            :X	1
            :[	-1
            :[	-1
            :\	-1
            :]	1
            :_(	-1
            :b)	1
            :l	0
            :o(	-1
            :o)	1
            :p	1
            :s	-1
            :|	-1
            :|	0
            :Þ	1
            :…(	-1
            ;)	0
            ;^)	1
            ;o)	0
            </3-1	-1
            <3	1
            <:}	0
            <o<	-1
            =(	-1
            =)	1
            =[	-1
            =]	1
            >/	-1
            >:(	-1
            >:)	1
            >:D	1
            >:L	-1
            >:O	-1
            >=D	1
            >[	-1
            >\	-1
            >o>	-1
            @}->--	1
            B(	-1
            Bc	-1
            D:	-1
            X(	-1
            X(	-1
            X-(	-1
            XD	1
            XD	1
            XO	-1
            XP	-1
            XP	1
            ^_^	1
            ^o)	-1
            x3?	1
            xD	1
            xP	-1
            |8C	-1
            |8c	-1
            |D	1
            }:)	1".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #region Identity Hate
        protected static string[] identityHateWords = {
            "uncivilised",
            "gypo",
            "gypos",
            "cunt",
            "cunts",
            "peckerwood",
            "peckerwoods",
            "raghead",
            "ragheads",
            "cripple",
            "cripples",
            "niggur",
            "niggurs",
            "yellow bone",
            "yellow bones",
            "muzzie",
            "muzzies",
            "niggar",
            "niggars",
            "nigger",
            "niggers",
            "greaseball",
            "greaseballs",
            "white trash",
            "white trashes",
            "nig nog",
            "nig nogs",
            "faggot",
            "faggots",
            "cotton picker",
            "cotton pickers",
            "darkie",
            "darkies",
            "hoser",
            "hosers",
            "Uncle Tom",
            "Uncle Toms",
            "Jihadi",
            "Jihadis",
            "retard",
            "retards",
            "hillbilly",
            "hillbillies",
            "fag",
            "fags",
            "trailer trash",
            "trailer trashes",
            "pikey",
            "pikies",
            "nicca",
            "niccas",
            "tranny",
            "trannies",
            "porch monkey",
            "porch monkies",
            "wigger",
            "wiggers",
            "wetback",
            "wetbacks",
            "nigglet",
            "nigglets",
            "wigga",
            "wiggas",
            "dhimmi",
            "dhimmis",
            "honkey",
            "honkies",
            "eurotrash",
            "eurotrashes",
            "yardie",
            "yardies",
            "trailer park trash",
            "trailer park trashes",
            "niggah",
            "niggahes",
            "yokel",
            "yokels",
            "nigguh",
            "nigguhes",
            "camel jockey",
            "camel jockies",
            "honkie",
            "honkies",
            "niglet",
            "niglets",
            "gyppo",
            "gyppos",
            "dyke",
            "dykes",
            "half breed",
            "honky",
            "honkies",
            "race traitor",
            "race traitors",
            "jiggaboo",
            "jiggaboos",
            "Chinaman",
            "Chinamans",
            "curry muncher",
            "curry munchers",
            "jungle bunny",
            "jungle bunnies",
            "coon ass",
            "coon asses",
            "newfie",
            "newfies",
            "house nigger",
            "house niggers",
            "limey",
            "limies",
            "red bone",
            "red bones",
            "guala",
            "gualas",
            "plastic paddy",
            "plastic paddies",
            "whigger",
            "whiggers",
            "jigaboo",
            "jigaboos",
            "nig",
            "nigs",
            "Zionazi",
            "Zionazis",
            "spear chucker",
            "spear chuckers",
            "niggress",
            "niggresses",
            "yobbo",
            "yobbos",
            "border jumper",
            "border jumpers",
            "sperg",
            "spergs",
            "pommy",
            "pommies",
            "munter",
            "munters",
            "tar baby",
            "tar babies",
            "pommie",
            "pommies",
            "gyp",
            "gyps",
            "anchor baby",
            "anchor babies",
            "twat",
            "twats",
            "border hopper",
            "border hoppers",
            "queer",
            "queers",
            "darky",
            "darkies",
            "ching chong",
            "ching chongs",
            "khazar",
            "khazars",
            "gippo",
            "gippos",
            "skanger",
            "skangers",
            "beaner",
            "beaners",
            "quadroon",
            "quadroons",
            "gator bait",
            "gator baits",
            "Cushite",
            "Cushites",
            "mud shark",
            "mud sharks",
            "cracker",
            "crackers",
            "dune coon",
            "dune coons",
            "pickaninny",
            "pickaninnies",
            "slant eye",
            "slant eyes",
            "sideways vagina",
            "sideways vaginas",
            "hick",
            "hicks",
            "camel fucker",
            "camel fuckers",
            "redneck",
            "rednecks",
            "spiv",
            "spivs",
            "zipperhead",
            "zipperheads",
            "Kushite",
            "Kushites",
            "Shylock",
            "Shylocks",
            "gook",
            "gooks",
            "papist",
            "papists",
            "hymie",
            "hymies",
            "wog",
            "wogs",
            "scally",
            "scallies",
            "coon",
            "coons",
            "whitey",
            "whities",
            "nigette",
            "nigettes",
            "paki",
            "pakis",
            "towel head",
            "towel heads",
            "Argie",
            "Argies",
            "wexican",
            "wexicans",
            "jigger",
            "jiggers",
            "injun",
            "injuns",
            "ocker",
            "ockers",
            "polack",
            "polacks",
            "moulie",
            "moulies",
            "niggor",
            "niggors",
            "scanger",
            "scangers",
            "ofay",
            "ofaies",
            "jigga",
            "jiggas",
            "redskin",
            "redskins",
            "chonky",
            "chonkies",
            "hebro",
            "hebros",
            "wop",
            "wops",
            "chink",
            "chinks",
            "sideways pussy",
            "sideways pussies",
            "paleface",
            "palefaces",
            "wagon burner",
            "wagon burners",
            "nigra",
            "nigras",
            "spic",
            "spics",
            "spics",
            "jocky",
            "jockies",
            "kraut",
            "krauts",
            "steek",
            "steeks",
            "coolie",
            "coolies",
            "gooky",
            "gookies",
            "octaroon",
            "octaroons",
            "bint",
            "bints",
            "shit heel",
            "shit heels",
            "squaw",
            "squaws",
            "bog trotter",
            "bog trotters",
            "Oriental",
            "Orientals",
            "halfrican",
            "halfricans",
            "paddy",
            "paddies",
            "groid",
            "groids",
            "jiggabo",
            "jiggabos",
            "jigg",
            "jiggs",
            "jant",
            "jants",
            "spide",
            "spides",
            "camel humper",
            "camel humpers",
            "white nigger",
            "white niggers",
            "ZOG",
            "ZOGs",
            "diaper head",
            "diaper heads",
            "heeb",
            "heebs",
            "Christ killer",
            "Christ killers",
            "piker",
            "pikers",
            "higger",
            "higgers",
            "lemonhead",
            "lemonheads",
            "Hun",
            "Huns",
            "popolo",
            "popolos",
            "cowboy killer",
            "cowboy killers",
            "jhant",
            "jhants",
            "eyetie",
            "eyeties",
            "mockey",
            "mockies",
            "alligator bait",
            "alligator baits",
            "Jap",
            "Japs",
            "shanty Irish",
            "shanty Irishes",
            "redlegs",
            "mulignan",
            "mulignans",
            "jockie",
            "jockies",
            "mangia cake",
            "mangia cakes",
            "moulinyan",
            "moulinyans",
            "nigar",
            "nigars",
            "darkey",
            "darkies",
            "gurrier",
            "gurriers",
            "lubra",
            "lubras",
            "Buckwheat",
            "Buckwheats",
            "mulato",
            "mulatos",
            "prairie nigger",
            "prairie niggers",
            "kyke",
            "kykes",
            "boonie",
            "boonies",
            "mick",
            "micks",
            "bluegum",
            "bluegums",
            "spigger",
            "spiggers",
            "border bunny",
            "border bunnies",
            "kike",
            "kikes",
            "moulignon",
            "moulignons",
            "roundeye",
            "roundeyes",
            "ginzo",
            "ginzos",
            "Jewbacca",
            "Jewbaccas",
            "booner",
            "booners",
            "nigre",
            "nigres",
            "scallie",
            "scallies",
            "niger",
            "nigers",
            "dinge",
            "dinges",
            "Leb",
            "Lebs",
            "Lebbo",
            "Lebbos",
            "sambo",
            "sambos",
            "Africoon",
            "Africoons",
            "ling ling",
            "ling lings",
            "gub",
            "gubs",
            "banana bender",
            "banana benders",
            "japie",
            "japies",
            "island nigger",
            "island niggers",
            "hairyback",
            "hairybacks",
            "lugan",
            "lugans",
            "Bog Irish",
            "Bog Irishes",
            "blaxican",
            "blaxicans",
            "moke",
            "mokes",
            "nigor",
            "nigors",
            "bix nood",
            "bix noods",
            "Kushi",
            "Kushis",
            "guala guala",
            "guala gualas",
            "hoosier",
            "hoosiers",
            "thicklips",
            "mook",
            "mooks",
            "muk",
            "muks",
            "soup taker",
            "soup takers",
            "senga",
            "sengas",
            "Cushi",
            "Cushis",
            "pogue",
            "pogues",
            "abo",
            "abos",
            "ping pang",
            "ping pangs",
            "proddy dog",
            "proddy dogs",
            "boong",
            "boongs",
            "dago",
            "dagos",
            "dogun",
            "doguns",
            "mocky",
            "mockies",
            "poppadom",
            "poppadoms",
            "Gwat",
            "Gwats",
            "ice nigger",
            "ice niggers",
            "spook",
            "spooks",
            "Afro-Saxon",
            "Afro-Saxons",
            "guido",
            "guidos",
            "latrino",
            "latrinos",
            "lowlander",
            "lowlanders",
            "mockie",
            "mockies",
            "moky",
            "mokies",
            "mosshead",
            "mossheads",
            "African catfish",
            "African catfishes",
            "gyppy",
            "gyppies",
            "timber nigger",
            "timber niggers",
            "Americoon",
            "Americoons",
            "camel cowboy",
            "camel cowboies",
            "eh hole",
            "eh holes",
            "Hunyak",
            "Hunyaks",
            "slopehead",
            "slopeheads",
            "teabagger",
            "teabaggers",
            "Armo",
            "Armos",
            "bitch",
            "bitches",
            "greaser",
            "greasers",
            "Honyock",
            "Honyocks",
            "mud person",
            "mud persons",
            "pineapple nigger",
            "pineapple niggers",
            "retarded",
            "semihole",
            "semiholes",
            "Amo",
            "Amos",
            "border nigger",
            "border niggers",
            "buckra",
            "buckras",
            "burrhead",
            "burrheads",
            "cab nigger",
            "cab niggers",
            "carpet pilot",
            "carpet pilots",
            "pancake face",
            "pancake faces",
            "spigotty",
            "spigotties",
            "carrot snapper",
            "carrot snappers",
            "chili shitter",
            "chili shitters",
            "curry slurper",
            "curry slurpers",
            "ghetto hamster",
            "ghetto hamsters",
            "ice monkey",
            "ice monkies",
            "roofucker",
            "roofuckers",
            "Velcro head",
            "Velcro heads",
            "wiggerette",
            "wiggerettes",
            "beach nigger",
            "beach niggers",
            "bean dipper",
            "bean dippers",
            "bog hopper",
            "bog hoppers",
            "Buddhahead",
            "Buddhaheads",
            "camel jacker",
            "camel jackers",
            "Caublasian",
            "Caublasians",
            "cave nigger",
            "cave niggers",
            "cow kisser",
            "cow kissers",
            "dune nigger",
            "dune niggers",
            "four by two",
            "four by twos",
            "fresh off the boat",
            "fresh off the boats",
            "gin jockey",
            "gin jockies",
            "golliwog",
            "golliwogs",
            "guinea",
            "guineas",
            "Jim Fish",
            "Jim Fishes",
            "mackerel snapper",
            "mackerel snappers",
            "octroon",
            "octroons",
            "pohm",
            "pohms",
            "pussy",
            "pussies",
            "Russellite",
            "Russellites",
            "spice nigger",
            "spice niggers",
            "uncivilized",
            "Whipped",
            "albino",
            "albinos",
            "ape",
            "apes",
            "Aunt Jemima",
            "Aunt Jemimas",
            "buckethead",
            "bucketheads",
            "Chinese wetback",
            "Chinese wetbacks",
            "chug",
            "chugs",
            "curry stinker",
            "curry stinkers",
            "dyke jumper",
            "dyke jumpers",
            "eight ball",
            "eight balls",
            "gun burglar",
            "gun burglars",
            "ikey mo",
            "ikey mos",
            "lawn jockey",
            "lawn jockies",
            "leprechaun",
            "leprechauns",
            "mutt",
            "mutts",
            "negro",
            "negros",
            "negroes",
            "nitchee",
            "nitchees",
            "sooty",
            "sooties",
            "spick",
            "spicks",
            "tinkard",
            "tinkards",
            "uncircumcised baboon",
            "uncircumcised baboons",
            "zigabo",
            "zigabos",
            "abbo",
            "abbos",
            "Anglo",
            "Anglos",
            "Aunt Jane",
            "Aunt Janes",
            "Aunt Mary",
            "Aunt Maries",
            "Aunt Sally",
            "Aunt Sallies",
            "azn",
            "azns",
            "bamboo coon",
            "bamboo coons",
            "banana lander",
            "banana landers",
            "banjo lips",
            "bans and cans",
            "beaner shnitzel",
            "beaner shnitzels",
            "beaney",
            "beanies",
            "Bengali",
            "Bengalis",
            "bhrempti",
            "bhremptis",
            "bird",
            "birds",
            "bitter clinger",
            "bitter clingers",
            "black Barbie",
            "black Barbies",
            "black dago",
            "black dagos",
            "blockhead",
            "blockheads",
            "bog jumper",
            "bog jumpers",
            "boon",
            "boons",
            "boonga",
            "boongas",
            "Bounty bar",
            "Bounty bars",
            "boxhead",
            "boxheads",
            "brass ankle",
            "brass ankles",
            "brownie",
            "brownies",
            "buffie",
            "buffies",
            "bug eater",
            "bug eaters",
            "buk buk",
            "buk buks",
            "bumblebee",
            "bumblebees",
            "bung",
            "bungs",
            "bunga",
            "bungas",
            "butterhead",
            "butterheads",
            "can eater",
            "can eaters",
            "celestial",
            "celestials",
            "Charlie",
            "Charlies",
            "chee chee",
            "chee chees",
            "chi chi",
            "chi chis",
            "chigger",
            "chiggers",
            "chinig",
            "chinigs",
            "chink a billy",
            "chink a billies",
            "chunky",
            "chunkies",
            "clam",
            "clams",
            "clamhead",
            "clamheads",
            "colored",
            "coloured",
            "crow",
            "crows",
            "dego",
            "degos",
            "dink",
            "dinks",
            "dogan",
            "dogans",
            "domes",
            "dot head",
            "dot heads",
            "eggplant",
            "eggplants",
            "Fairy",
            "Fairies",
            "fez",
            "fezs",
            "FOB",
            "FOBs",
            "fog nigger",
            "fog niggers",
            "fuzzy",
            "fuzzies",
            "fuzzy wuzzy",
            "fuzzy wuzzies",
            "gable",
            "gables",
            "Gerudo",
            "Gerudos",
            "gew",
            "gews",
            "ghetto",
            "ghettos",
            "gipp",
            "gipps",
            "gook eye",
            "gook eyes",
            "gyppie",
            "gyppies",
            "heinie",
            "heinies",
            "ho",
            "hos",
            "hoe",
            "hoes",
            "Honyak",
            "Honyaks",
            "Hunkie",
            "Hunkies",
            "Hunky",
            "Hunkies",
            "Hunyock",
            "Hunyocks",
            "ike",
            "ikes",
            "ikey",
            "ikies",
            "iky",
            "ikies",
            "jig",
            "jigs",
            "jigarooni",
            "jigaroonis",
            "jijjiboo",
            "jijjiboos",
            "kotiya",
            "kotiyas",
            "mickey",
            "mickies",
            "moch",
            "moches",
            "mock",
            "mocks",
            "mong",
            "mongs",
            "monkey",
            "monkies",
            "Moor",
            "Moors",
            "moss eater",
            "moss eaters",
            "moxy",
            "moxies",
            "muktuk",
            "muktuks",
            "mung",
            "mungs",
            "munt",
            "munts",
            "ned",
            "net head",
            "net heads",
            "nichi",
            "nichis",
            "nichiwa",
            "nichiwas",
            "nidge",
            "nidges",
            "nip",
            "nips",
            "nitchie",
            "nitchies",
            "nitchy",
            "nitchies",
            "Orangie",
            "Orangies",
            "Oreo",
            "Oreos",
            "papoose",
            "papooses",
            "piky",
            "pikies",
            "pinto",
            "pintos",
            "pointy head",
            "pointy heads",
            "pollo",
            "pollos",
            "pom",
            "poms",
            "pommie grant",
            "pommie grants",
            "Punjab",
            "Punjabs",
            "rube",
            "rubes",
            "sawney",
            "sawnies",
            "scag",
            "scags",
            "seppo",
            "seppos",
            "septic",
            "septics",
            "shant",
            "shants",
            "sheeny",
            "sheenies",
            "sheepfucker",
            "sheepfuckers",
            "Shelta",
            "Sheltas",
            "shiner",
            "shiners",
            "shit kicker",
            "shit kickers",
            "Shy",
            "Shies",
            "sideways cooter",
            "sideways cooters",
            "skag",
            "skags",
            "Skippy",
            "Skippies",
            "slag",
            "slags",
            "slant",
            "slants",
            "slit",
            "slits",
            "slope",
            "slopes",
            "slopey",
            "slopies",
            "slopy",
            "slopies",
            "smoke jumper",
            "smoke jumpers",
            "smoked Irish",
            "smoked Irishes",
            "smoked Irishman",
            "smoked Irishmans",
            "sole",
            "soles",
            "spickaboo",
            "spickaboos",
            "spig",
            "spigs",
            "spik",
            "spiks",
            "spink",
            "spinks",
            "squarehead",
            "squareheads",
            "squinty",
            "squinties",
            "stovepipe",
            "stovepipes",
            "sub human",
            "sub humans",
            "sucker fish",
            "sucker fishes",
            "Taffy",
            "Taffies",
            "teapot",
            "teapots",
            "tenker",
            "tenkers",
            "tincker",
            "tinckers",
            "tinkar",
            "tinkars",
            "tinker",
            "tinkers",
            "tinkere",
            "tinkeres",
            "trash",
            "trashes",
            "tree jumper",
            "tree jumpers",
            "tunnel digger",
            "tunnel diggers",
            "Twinkie",
            "Twinkies",
            "tyncar",
            "tyncars",
            "tynekere",
            "tynekeres",
            "tynkard",
            "tynkards",
            "tynkare",
            "tynkares",
            "tynker",
            "tynkers",
            "tynkere",
            "tynkeres",
            "WASP",
            "WASPs",
            "Yank",
            "Yanks",
            "Yankee",
            "Yankees",
            "yellow",
            "yellows",
            "yid",
            "yids",
            "yob",
            "yobs",
            "zebra",
            "zebras",
            "zippohead",
            "zippoheads",
            "ZOG lover",
            "ZOG lovers",
            "knacker",
            "knackers",
            "shyster",
            "shysters",
            "bogan",
            "bogans",
            "hayseed",
            "moon cricket",
            "moon crickets",
            "mud duck",
            "mud ducks",
            "surrender monkey",
            "surrender monkies",
            "bludger",
            "bludgers",
            "charver",
            "charvers",
            "dole bludger",
            "dole bludgers",
            "chav",
            "chavs",
            "sheister",
            "sheisters",
            "charva",
            "charvas",
            "touch of the tar brush",
            "touch of the tar brushes",
            "Northern monkey",
            "Northern monkies",
            "Southern fairy",
            "Southern fairies",
            "gubba",
            "gubbas",
            "stump jumper",
            "stump jumpers",
            "hebe",
            "hebes",
            "millie",
            "millies",
            "quashie",
            "quashies",
            "dingo fucker",
            "dingo fuckers",
            "mil bag",
            "mil bags",
            "conspiracy theorist",
            "conspiracy theorists",
            "whore from Fife",
            "whore from Fifes",
            "boojie",
            "boojies",
            "book book",
            "book books",
            "cheese eating surrender monkey",
            "cheese eating surrender monkies",
            "idiot",
            "idiots",
            "jock",
            "jocks",
            "mack",
            "macks",
            "Merkin",
            "Merkins",
            "neche",
            "neches",
            "neejee",
            "neejees",
            "neechee",
            "neechees",
            "powderburn",
            "powderburns",
            "proddywhoddy",
            "proddywhoddies",
            "proddywoddy",
            "proddywoddies",
            "Rhine monkey",
            "Rhine monkies" };
        #endregion

        #region Hate Phrases
        protected static string[] identityHatePhrases =
            @"allah akbar,
            blacks,
            chink,
            chinks,
            dykes,
            faggot,
            faggots,
            fags,
            homo,
            inbred,
            nigger,
            niggers,
            queers,
            raped,
            savages,
            slave,
            spic,
            wetback,
            wetbacks,
            whites,
            a dirty,
            a nigger,
            all niggers,
            all white,
            always fuck,
            ass white,
            be killed,
            beat him,
            biggest faggot,
            blame the,
            butt ugly,
            chink eyed,
            chinks in,
            coon shit,
            dumb monkey,
            dumb nigger,
            fag and,
            fag but,
            faggot a,
            faggot and,
            faggot ass,
            faggot bitch,
            faggot for,
            faggot smh,
            faggot that,
            faggots and,
            faggots like,
            faggots usually,
            faggots who,
            fags are,
            fuckin faggot,
            fucking faggot,
            fucking gay,
            fucking hate,
            fucking nigger,
            fucking queer,
            gay ass,
            get raped,
            hate all,
            hate faggots,
            hate fat,
            hate you,
            here faggot,
            is white,
            jungle bunny,
            kill all,
            kill yourself,
            little faggot,
            many niggers,
            married to,
            me faggot,
            my coon,
            nigga ask,
            niggas like,
            nigger ass,
            nigger is,
            nigger music,
            niggers are,
            of fags,
            of white,
            raped and,
            raped by,
            sand nigger,
            savages that,
            shorty bitch,
            spear chucker,
            spic cop,
            stupid nigger,
            that fag,
            that faggot,
            that nigger,
            the faggots,
            the female,
            the niggers,
            their heads,
            them white,
            then faggot,
            this nigger,
            to rape,
            trailer park,
            trash with,
            u fuckin,
            ugly dyke,
            up nigger,
            white ass,
            white boy,
            white person,
            white trash,
            with niggas,
            you fag,
            you nigger,
            you niggers,
            your faggot,
            your nigger,
            a bitch made,
            a fag and,
            a fag but,
            a faggot and,
            a faggot for,
            a fucking queer,
            a nigga ask,
            a white person,
            a white trash,
            all these fucking,
            are all white,
            be killed for,
            bitch made nigga,
            faggots like you,
            faggots usually have,
            fuck outta here,
            fuck u talking,
            fuck you too,
            fucking hate you,
            full of white,
            him a nigga,
            his shorty bitch,
            how many niggers,
            is a fag,
            is a faggot,
            is a fuckin,
            is a fucking,
            is a nigger,
            like a faggot,
            like da colored,
            many niggers are,
            nigga and his,
            niggers are in,
            of white trash,
            shut up nigger,
            still a faggot,
            the biggest faggot,
            the faggots who,
            the fuck do,
            they all look,
            what a fag,
            white bitch in,
            white trash and,
            you a fag,
            you a lame,
            you a nigger,
            you fuck wit,
            you fucking faggot,
            your a cunt,
            your a dirty,
            your bitch in,
            a bitch made nigga,
            a lame nigga you,
            faggot if you ever,
            full of white trash,
            how many niggers are,
            is full of white,
            lame nigga you a,
            many niggers are in,
            nigga you a lame,
            niggers are in my,
            wit a lame nigga,
            you a lame bitch,
            you fuck wit a".Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        #endregion

        #endregion

        #endregion
    }
}
