using System;
using System.Collections.Generic;

//go see creature bird dwarf, I should absolutely separate these
namespace Scryfalltypes
{
    using UUID = System.String;
    using Colors = IEnumerable<Color>;
    public enum Color { W, U, G, B, R, C }
    public class Card
    {
        public int? arena_id; //This card’s Arena ID, if any. A large percentage of cards are not available on Arena and do not have this ID.
        public UUID id; //A unique ID for this card in Scryfall’s database. 
        public string lang; //A language code for this printing.
        public int? mtgo_id; //This card’s Magic Online ID (also known as the Catalog ID), if any. A large percentage of cards are not available on Magic Online and do not have this ID.
        public int? mtgo_foil_id; //This card’s foil Magic Online ID (also known as the Catalog ID), if any. A large percentage of cards are not available on Magic Online and do not have this ID.
        public IEnumerable<int> multiverse_ids; //This card’s multiverse IDs on Gatherer, if any, as an array of integers. Note that Scryfall includes many promo cards, tokens, and other esoteric objects that do not have these identifiers.
        public int? tcgplayer_id; //This card’s ID on TCGplayer’s API, also known as the productId.
        public int? tcgplayer_etched_id; //This card’s ID on TCGplayer’s API, for its etched version if that version is a separate product.
        public int? cardmarket_id; //This card’s ID on Cardmarket’s API, also known as the idProduct.
        public string @object; //A content type for this object, always card.
        public UUID oracle_id; //A unique ID for this card’s oracle identity. This value is consistent across reprinted card editions, and unique among different cards with the same name (tokens, Unstable variants, etc).
        public Uri prints_search_uri; //A link to where you can begin paginating all re/prints for this card on Scryfall’s API.
        public Uri rulings_uri; //A link to this card’s rulings list on Scryfall’s API.
        public Uri scryfall_uri; //A link to this card’s permapage on Scryfall’s website.
        public Uri uri; //A link to this card object on Scryfall’s API. 
        #region gameplay
        public IEnumerable<RelatedCard> all_parts; //If this card is closely related to other cards, this property will be an array with Related Card Objects.
        public IEnumerable<CardFace> card_faces; //An array of Card Face objects, if this card is multifaced.
        public decimal cmc; //The card’s converted mana cost. Note that some funny cards have fractional mana costs.
        public Colors color_identity; //This card’s color identity.
        public Colors color_indicator; //The colors in this card’s color indicator, if any. A null value for this field indicates the card does not have one.
        public Colors colors; //This card’s colors, if the overall card has colors defined by the rules. Otherwise the colors will be on the card_faces objects, see below.
        public int? edhrec_rank; //This card’s overall rank/popularity on EDHREC. Not all cards are ranked.
        public string hand_modifier; //This card’s hand modifier, if it is Vanguard card. This value will contain a delta, such as -1.
        public IEnumerable<string> keywords; //An array of keywords that this card uses, such as 'Flying' and 'Cumulative upkeep'.
        public string layout; //A code for this card’s layout.
        public Legalities legalities; //An object describing the legality of this card across play formats. Possible legalities are legal, not_legal, restricted, and banned.
        public string life_modifier; //This card’s life modifier, if it is Vanguard card. This value will contain a delta, such as +2.
        public string loyalty; //This loyalty if any. Note that some cards have loyalties that are not numeric, such as X.
        public string mana_cost; //The mana cost for this card. This value will be any empty string "" if the cost is absent. Remember that per the game rules, a missing mana cost and a mana cost of {0} are different values. Multi-faced cards will report this value in card faces.
        public string name; //The name of this card. If this card has multiple faces, this field will contain both names separated by ␣//␣.
        public string oracle_text; //The Oracle text for this card, if any.
        public bool oversized; //True if this card is oversized.
        public string power; //This card’s power, if any. Note that some cards have powers that are not numeric, such as *.
        public Colors produced_mana; //Colors of mana that this card could produce.
        public bool reserved; //True if this card is on the Reserved List.
        public string toughness; //This card’s toughness, if any. Note that some cards have toughnesses that are not numeric, such as *.
        public string type_line; //The type line of this card. 
        #endregion
        #region print
        public string artist; //The name of the illustrator of this card. Newly spoiled cards may not have this field yet.
        public bool booster; //Whether this card is found in boosters.
        public string border_color; //This card’s border color: black, white, borderless, silver, or gold.
        public UUID card_back_id; //The Scryfall ID for the card back design present on this card.
        public string collector_number; //This card’s collector number. Note that collector numbers can contain non-numeric characters, such as letters or ★.
        public bool? content_warning; //True if you should consider avoiding use of this print downstream.
        public bool digital; //True if this card was only released in a video game.
        public IEnumerable<string> finishes; //An array of computer-readable flags that indicate if this card can come in foil, nonfoil, etched, or glossy finishes.
        public string flavor_name; //The just-for-fun name printed on the card (such as for Godzilla series cards).
        public string flavor_text; //The flavor text, if any.
        public IEnumerable<string> frame_effects; //This card’s frame effects, if any.
        public string frame; //This card’s frame layout.
        public bool full_art; //True if this card’s artwork is larger than normal.
        public IEnumerable<string> games; //A list of games that this card print is available in, paper, arena, and/or mtgo.
        public bool highres_image; //True if this card’s imagery is high resolution.
        public UUID illustration_id; //A unique identifier for the card artwork that remains consistent across reprints. Newly spoiled cards may not have this field yet.
        public string image_status; //A computer-readable indicator for the state of this card’s image, one of missing, placeholder, lowres, or highres_scan.
        public Images image_uris; //An object listing available imagery for this card. See the Card Imagery article for more information.
        public Dictionary<string, Uri> prices; //An object containing daily price information for this card, including usd, usd_foil, usd_etched, eur, and tix prices, as strings.
        public string printed_name; //The localized name printed on this card, if any.
        public string printed_text; //The localized text printed on this card, if any.
        public string printed_type_line; //The localized type line printed on this card, if any.
        public bool promo; //True if this card is a promotional print.
        public IEnumerable<string> promo_types; //An array of strings describing what categories of promo cards this card falls into.
        public Dictionary<string, Uri> purchase_uris; //An object providing URIs to this card’s listing on major marketplaces.
        public string rarity; //This card’s rarity. One of common, uncommon, rare, special, mythic, or bonus.
        public Dictionary<string, Uri> related_uris; //An object providing URIs to this card’s listing on other Magic: The Gathering online resources.
        public DateTime released_at; //The date this card was first released.
        public bool reprint; //True if this card is a reprint.
        public Uri scryfall_set_uri; //A link to this card’s set on Scryfall’s website.
        public string set_name; //This card’s full set name.
        public Uri set_search_uri; //A link to where you can begin paginating this card’s set on the Scryfall API.
        public string set_type; //The type of set this printing is in.
        public Uri set_uri; //A link to this card’s set object on Scryfall’s API.
        public string set; //This card’s set code.
        public string set_id; //This card’s Set object UUID.
        public bool story_spotlight; //True if this card is a Story Spotlight.
        public bool textless; //True if the card is printed without text.
        public bool variation; //Whether this card is a variation of another printing.
        public UUID variation_of; //The printing ID of the printing this card is a variation of.
        public string watermark; //This card’s watermark, if any.
        public Preview preview;
        #endregion
    }
    public class CardFace
    {
        public string artist { get; set; } //The name of the illustrator of this card face. Newly spoiled cards may not have this field yet.
        public Colors color_indicator { get; set; } //The colors in this face’s color indicator, if any.
        public Colors colors { get; set; } //This face’s colors, if the game defines colors for the individual face of this card.
        public string flavor_text { get; set; } //The flavor text printed on this face, if any.
        public UUID illustration_id { get; set; } //A unique identifier for the card face artwork that remains consistent across reprints. Newly spoiled cards may not have this field yet.
        public Images image_uris { get; set; } //An object providing URIs to imagery for this face, if this is a double-sided card. If this card is not double-sided, then the image_uris property will be part of the parent object instead.
        public string loyalty { get; set; } //This face’s loyalty, if any.
        public string mana_cost { get; set; } //The mana cost for this face. This value will be any empty string "" if the cost is absent. Remember that per the game rules, a missing mana cost and a mana cost of {0} are different values.
        public string name { get; set; } //The name of this particular face.
                                         //public  string object { get; set; } //A content type for this object, always card_face.
        public string oracle_text { get; set; } //The Oracle text for this face, if any.
        public string power { get; set; } //This face’s power, if any. Note that some cards have powers that are not numeric, such as *.
        public string printed_name { get; set; } //The localized name printed on this face, if any.
        public string printed_text { get; set; } //The localized text printed on this face, if any.
        public string printed_type_line { get; set; } //The localized type line printed on this face, if any.
        public string toughness { get; set; } //This face’s toughness, if any.
        public string type_line { get; set; } //The type line of this particular face.
        public string watermark { get; set; } //The watermark on this particulary card face, if any. 
    }
    public class RelatedCard
    {
        public UUID id { get; set; } //An unique ID for this card in Scryfall’s database.
                                     //public string object { get; set; } //A content type for this object, always related_card.
        public string component { get; set; } //A field explaining what role this card plays in this relationship, one of token, meld_part, meld_result, or combo_piece.
        public string name { get; set; } //The name of this particular related card.
        public string type_line { get; set; } //The type line of this card.
        public Uri uri { get; set; } //A URI where you can retrieve a full object describing this card on Scryfall’s API. 
    }
    public enum LegalCategory { not_legal, legal, restricted, banned }
    public class Legalities
    {
        public LegalCategory standard { get; set; }
        public LegalCategory future { get; set; }
        public LegalCategory historic { get; set; }
        public LegalCategory gladiator { get; set; }
        public LegalCategory pioneer { get; set; }
        public LegalCategory modern { get; set; }
        public LegalCategory legacy { get; set; }
        public LegalCategory pauper { get; set; }
        public LegalCategory vintage { get; set; }
        public LegalCategory penny { get; set; }
        public LegalCategory commander { get; set; }
        public LegalCategory brawl { get; set; }
        public LegalCategory historicbrawl { get; set; }
        public LegalCategory paupercommander { get; set; }
        public LegalCategory duel { get; set; }
        public LegalCategory oldschool { get; set; }
        public LegalCategory premodern { get; set; }
    }

    public class Images
    {
        public Uri png { get; set; } // 745 × 1040, PNG, A transparent, rounded full card PNG. This is the best image to use for videos or other high-quality content.
        public Uri border_crop { get; set; } //480 × 680, JPG, A full card image with the rounded corners and the majority of the border cropped off. Designed for dated contexts where rounded images can’t be used.
        public Uri art_crop { get; set; } //Varying resolution, JPG, A rectangular crop of the card’s art only. Not guaranteed to be perfect for cards with outlier designs or strange frame arrangements
        public Uri large { get; set; } //672 × 936, JPG, A large full card image
        public Uri normal { get; set; } //488 × 680, JPG, A medium-sized full card image
        public Uri small { get; set; } //146 × 204, JPG, A small full card image. Designed for use as thumbnail or list icon.
    }

    public class Preview
    {
        public DateTime previewed_at; //The date this card was previewed.
        public Uri source_uri; //A link to the preview for this card.
        public string source; //The name of the source that previewed this card.
    }
}