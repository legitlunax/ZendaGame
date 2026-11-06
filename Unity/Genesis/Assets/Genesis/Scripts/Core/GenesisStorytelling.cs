using System;
using System.Collections.Generic;
using System.Linq;

namespace Genesis
{
    public sealed class GenesisStoryPage
    {
        public string Text { get; }
        public string Mood { get; }
        public string Speaker { get; }
        public string SpeakerIconId { get; }
        public GenesisStoryPage(string text, string mood, string speaker = "Zenda", string speakerIconId = null)
        { Text = text; Mood = mood; Speaker = speaker; SpeakerIconId = speakerIconId; }
    }

    // Presentation-only retelling. Original lore, chapter IDs and economic goals are untouched.
    public static class GenesisStorytelling
    {
        public const int PageLength = 220;

        public static List<GenesisStoryPage> Build(GenesisCatalog catalog, ChapterDefinition chapter,
            Func<string, string> translate = null, bool paginate = true)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (chapter == null) throw new ArgumentNullException(nameof(chapter));
            var pages = new List<GenesisStoryPage>();
            GeneratorDefinition creation = catalog.generators.FirstOrDefault(g => g.id == chapter.kind);
            // Elpis joins the present-day retelling even when recalling the time before her awakening.
            GeneratorDefinition guestCreation = creation ?? catalog.generators.FirstOrDefault(g => g.id == "hope_fluctuation");
            ResidentFriendDefinition guest = guestCreation == null || guestCreation.resident_friends == null
                ? null : guestCreation.resident_friends.FirstOrDefault();
            if (guest == null)
            {
                guestCreation = catalog.generators.FirstOrDefault(g => g.resident_friends != null && g.resident_friends.Any());
                guest = guestCreation == null ? null : guestCreation.resident_friends.First();
            }
            string opening, participation, mood = "surprised";
            switch (chapter.id)
            {
                case "first_light":
                    opening = "Come closer. I want to tell you how our creation began. I am Zenda, and I remember a silence so complete that even my own light had nowhere to fall.";
                    participation = "I cupped your first spark of hope in my hands. I could not yet make a world, but I could protect that spark. Together, we gave nothingness its first promise: something could be.";
                    mood = "sad";
                    break;
                case "first_possibility":
                    opening = "Do you remember Lady Elpis? I felt her before I saw her: a tiny trembling possibility in the dark. I opened my hands so she would have somewhere safe to awaken.";
                    participation = "I gathered the scattered sparks around Lady Elpis, but I did not choose her path for her. She taught me to nurture a possibility without deciding everything it must become.";
                    mood = "happy";
                    break;
                case "discover_first_intention":
                    opening = "Possibilities were drifting everywhere. I reached toward them, then hesitated. What did I actually want to create? Beside me, Lady Votia turned that question into our first intention.";
                    participation = "I chose to begin rather than remain only a dreamer. With Lady Votia, I gave our hope a direction: let there be something. That choice was small, but I still carry its courage.";
                    break;
                case "discover_primordial_breath":
                    opening = "I remember holding my breath, afraid that one careless movement would scatter everything. Lady Aeria smiled at me. Creation, she reminded me, could not grow if I held it perfectly still.";
                    participation = "I breathed with Lady Aeria. We sent that first ripple through the silence together, gently enough to carry our fragile beginnings rather than sweep them away.";
                    mood = "relaxed";
                    break;
                case "discover_yin":
                    opening = "Not every act of creation needed more light. I was tired from reaching outward, and Lady Yuelin showed me the shelter that could live within stillness.";
                    participation = "I helped Lady Yuelin tend a quiet sanctuary. There, I learned to receive and to listen. My part was not to fill every silence, but to make room for another presence.";
                    mood = "relaxed";
                    break;
                case "discover_yang":
                    opening = "Then Lady Riyara arrived like the warmth of a sunrise. I had learned how to shelter a spark; now I had to let it shine beyond my hands.";
                    participation = "I stepped into that radiance with Lady Riyara. Together we awakened motion and expression, while I kept Lady Yuelin's lesson close: warmth should invite life, never demand that it burn.";
                    mood = "happy";
                    break;
                case "discover_yin_yang":
                    opening = "I stood between Lady Yuelin's stillness and Lady Riyara's radiance. For a moment I feared that one would have to yield. Lady Lianghua showed me a more generous beginning.";
                    participation = "I helped weave a meeting place where receiving and expressing could sustain each other. I did not merge their voices into one. I held the space in which both could answer.";
                    mood = "relaxed";
                    break;
                default:
                    string companion = creation == null || string.IsNullOrWhiteSpace(creation.fairy) ? "my companions" : creation.fairy;
                    string name = creation == null ? chapter.title : creation.name;
                    opening = "Let me take you back to " + name + ". I was there with " + companion + ", learning what this new part of creation needed from me.";
                    participation = Participation(creation);
                    mood = Emotion(creation);
                    break;
            }
            Add(pages, opening, mood);
            if (guest != null)
                Add(pages, chapter.id == "first_light"
                    ? "Zenda, I had not awakened yet when you held that first spark. But hearing you tell it now, I can picture your hands around it. Will you tell me what you hoped we might become?"
                    : "Do you remember our first visit, Zenda? We were still learning what " + guest.home + " could become. I remember you staying beside me, not giving orders from far away.", "happy", guest.name, GuestIcon(guestCreation, guest));
            // Keep the full original account, including its fictional-world and freedom-of-belief framing.
            Add(pages, "This is the moment I remember:\n\n" + (chapter.lore ?? "A new possibility opened before us."), mood,
                guest == null ? "Zenda" : guest.name, guest == null ? null : GuestIcon(guestCreation, guest));
            string[] challenge = GenesisStoryChallenges.Build(creation);
            Add(pages, challenge[0], "surprised");
            Add(pages, challenge[1], "relaxed", guest == null ? "Zenda" : guest.name,
                guest == null ? null : GuestIcon(guestCreation, guest));
            Add(pages, challenge[2], "happy");
            Add(pages, participation, "relaxed");
            if (guest != null)
                Add(pages, LadyRecollection(creation, guest), "relaxed", guest.name, GuestIcon(guestCreation, guest));
            string continuityStory = LifeContinuityStory(chapter.kind);
            if (!string.IsNullOrEmpty(continuityStory))
            {
                Add(pages, continuityStory, "relaxed");
                if (guest != null)
                    Add(pages, ContinuityReply(chapter.kind), "relaxed", guest.name, GuestIcon(guestCreation, guest));
                Add(pages, "Yes. That is why I wanted us to tell this memory together. You were there beside me; your part of our adventure matters just as much as mine.", "happy");
            }
            if (creation != null && creation.resident_friends != null)
                foreach (ResidentFriendDefinition friend in creation.resident_friends)
                {
                    if (friend.name == "Lady Ignis")
                        Add(pages, "I visited the Fire Dimension and met Lady Ignis. Together we built hearths where her flames could warm rather than wound. She laughed when my first little spark went out, then taught me to try again.", "happy");
                    else if (friend.home == "Death Dimension")
                        Add(pages, "Beyond the Soul Dimension, I met " + friend.name + " in the Death Dimension. I helped her build a quiet garden of farewells. She tended each ending with care; I learned that even goodbye need not be lonely.", "relaxed");
                    else
                        Add(pages, "I visited " + friend.home + " and helped " + friend.name + " construct her home. She showed me what this place needed, and we worked side by side. That was how our friendship began.", "happy");
                    Add(pages, "And I remember you coming back, Zenda. " + friend.home + " is my home, but there is always room for a visit from my friend. We built it together; we still have more adventures to share.", "happy", friend.name, GuestIcon(creation, friend));
                    Add(pages, friend.name + " lives in " + friend.home + ". She is my friend, not my possession. When I visit again, we share our stories and tend what we built together. Her home remains hers when I travel onward.", "relaxed");
                    if (!string.IsNullOrWhiteSpace(friend.title))
                        Add(pages, "I call her " + friend.name + ", " + friend.title + ". Her name reminds me of what we learned together, not a command over everyone who lives in her realm.", "relaxed");
                }
            if (creation != null)
                foreach (GeneratorDefinition resident in catalog.generators.Where(g => g.recruit_after == creation.id))
                {
                    Add(pages, resident.desc, "happy", resident.fairy, resident.id);
                    Add(pages, resident.fairy + " makes her home in " + resident.dimension
                        + ". We can visit her there and invite her to gather alongside her neighbours, each at her own pace.", "relaxed");
                }
            if (paginate)
            {
                var localized = new List<GenesisStoryPage>();
                foreach (GenesisStoryPage passage in pages)
                    Paginate(localized, translate == null ? passage.Text : translate(passage.Text),
                        passage.Mood, passage.Speaker, passage.SpeakerIconId);
                return localized;
            }
            return pages;
        }

        private static string GuestIcon(GeneratorDefinition creation, ResidentFriendDefinition friend)
        { return friend.home == "Death Dimension" ? "shadow_realm" : creation.id; }

        public static string LadyRecollection(GeneratorDefinition creation, ResidentFriendDefinition friend)
        {
            if (friend.home == "Death Dimension")
                return "Zenda, I remember our garden of farewells. You stayed to listen instead of hurrying anyone through a goodbye. I tend those thresholds, but I cannot erase a living being. Life keeps everyone safely themselves in our imagined creation.";
            if (creation == null)
                return "I love hearing that beginning, Zenda. You could not see who would answer your hope. Now I can sit beside you and answer for myself. Our friendship grew from that little beginning.";
            switch (creation.era)
            {
                case "Five Elements":
                    return "I remember testing the boundaries with you, Zenda. When our work touched another element, you paused and listened. We learned how to meet our neighbours without losing what made this home ours.";
                case "Time":
                    return "You waited with me, Zenda. I remember how strange it felt to have a before to talk about! We learned the rhythm together, rather than making every world hurry to the same beat.";
                case "Space":
                    return "I remember calling across that first distance, Zenda, and hearing you answer. What we made was not just room to be apart. It was a place where friends could find each other again.";
                case "Causal Weave":
                    return "I could feel you holding the threads steady, Zenda. We were afraid of tangling everything together. Instead, we found a way for time and space to meet without either losing its own story.";
                case "Quantum Foundations":
                    return "I remember those delicate patterns, Zenda. You gave me time to try again when a shape would not hold. These are memories of our fantasy world, not real-world physics, but our patience was part of the adventure.";
                case "Matter and Stars":
                    return "Do you remember waiting for that first light, Zenda? I watched you gather what had scattered. Neither of us could hurry it. When it finally shone, I wanted you to be the first to see it with me.";
                case "Worlds and Spirit":
                    return "I remember looking around " + friend.home + " with you, Zenda. You asked what would make it a home for those who lived here. I still think of that question whenever I tend what we built.";
                case "Universal Emotions":
                    return "You listened to me, Zenda, even when neither of us had the right words. I remember being allowed to feel without being told what I ought to feel. That quiet adventure changed our friendship.";
                default:
                    return "I remember showing you " + friend.home + ", Zenda. You asked what I wanted to build, and helped me find my own answer. When I look at " + creation.name + " now, I remember our work and the conversations that made us friends.";
            }
        }

        private static string ContinuityReply(string kind)
        {
            switch (kind)
            {
                case "gaia_biosphere": return "I checked the smallest roots afterwards, Zenda. Every creature was still itself. That is what I remember most: our garden had not traded anyone away to bloom again.";
                case "soul_dawn": return "I remember that light in my hands, Zenda. It was a whole living being, not a fading trace. Even a farewell could not put anyone beyond Life's care.";
                case "parallel_universe": return "You looked at my cup first, Zenda! I was still finishing the same sentence. Our universe was exactly as before, and I was still me, sharing tea with my friend.";
                case "omniverse": return "I remember your hand beside mine when the skies returned, Zenda. No universe had survived to rescue the others. Life had kept everyone safe without needing one.";
                case "life_memory": return "I remember the private jokes too, Zenda. We did not rebuild people from a list of names. Each person continued their own story, whether we knew them or not.";
                case "divine_life": return "I remember returning to the ocean with you, Zenda. Our vision was not an invitation to seek danger. We cared for the creatures before us, while telling the promise of our imagined world.";
                default: return "And I remember your questions, Zenda. The unnoticed stranger was as safe as either of us. Our story's promise had no exception for someone who had never heard our names.";
            }
        }

        // Shared by Chronicles and resident histories; presentation only, never a gameplay reward.
        public static string LifeContinuityStory(string kind)
        {
            switch (kind)
            {
                case "gaia_biosphere":
                    return "The Garden That Could Not Be Lost\n\nI knelt beside Lady Florielle when a storm of unmaking reached our first garden. Before it could touch a root, Life sheltered every living being: the unseen soil dwellers, the birds, and the smallest sleeping seed. Nothing living was left behind.\n\nWhere the storm had passed, the same garden returned, down to the dew on each leaf. Its inhabitants were the same beings, not replacements. I understood our creation's promise: Life's protection belongs to everyone, without needing strength, worship, or permission.";
                case "soul_dawn":
                    return "The Farewell That Was Not Erasure\n\nI asked Lady Animelle whether destruction could scatter someone beyond finding. She held a small light and showed me an identity that no broken world could undo. Life kept the whole being safe, with every memory and every bond.\n\nBeside Lady Morthelia, I made a garden where people could meet through changes and farewells. Death could tend a threshold, but could never erase a person or overrule Life's shelter. In our fantasy, no one was sacrificed so that another might continue. Every living being remained themselves.";
                case "parallel_universe":
                    return "The Moment Between Universes\n\nI was sharing tea with Lady Mirielle when our universe vanished from one moment to the next. Before destruction could reach anyone, Life held every living being safely outside its reach. No one suffered; no one was missing.\n\nLife brought forth a new universe exactly the same as the previous one: the same stars, homes, histories, and unfinished cup of tea. Everyone continued as the same living being, with memories, identities, relationships, and choices intact, not as a copy left in someone else's place. I heard Mirielle finish her sentence, and knew that not even an instant of ruin could take her from herself.";
                case "omniverse":
                    return "When Every Sky Went Dark\n\nI stood with Lady Omnielle when destruction overtook every universe at once. There was no distant world left to flee to. Yet Life did not depend on a surviving universe: it sheltered all living beings safely, beyond destruction's reach.\n\nLife renewed every universe exactly as it had been, with all its inhabitants and all their connections intact. No world had to borrow another's people, and no one was forgotten between infinities. I watched every sky return. However vast the ending, Life's care was vaster; it needed no last refuge to remain unbroken.";
                case "life_memory":
                    return "Not One Name Forgotten\n\nI sat beside Lady Memorielle as she listened to the lives held safe through a world's renewal. She remembered more than names: a child's private joke, a creature's familiar path, a friendship not yet spoken aloud. Life preserved the whole living being, not merely a record from which someone else could be made.\n\nWhen the same world returned, each being continued their own unfinished story. I did not choose whose memories mattered. No one needed to be remembered by a goddess to be safe; Life forgot no one.";
                case "divine_life":
                    return "The Pursuit That Could Never Catch Life\n\nI walked with Lady Vivielle through a vision of destruction following Life from universe to universe. Each time a sky disappeared, Life already held every inhabitant safe and renewed their universe exactly as before. Even endings repeated without limit could neither exhaust its care nor claim a single being.\n\nWe did not ask anyone to prove this by facing danger. I stayed beside the first ocean and tended its creatures with her. This was a law of our imagined creation, not a promise about real-world death: here, Life's unbreakable shelter was freely given, never a test of courage or belief.";
                case "omniall":
                    return "The Promise Without an Exception\n\nI asked Lady Omnivielle about a life too small to notice, a stranger no one knew, and someone who had never believed in us. All were held in the same safety. Life did not sort the worthy from the unwanted, and it did not trade one being for another.\n\nEven if every universe were destroyed in a single moment, Life would remain, keeping all living beings safely themselves in new universes exactly the same as before. Death and destruction could follow, but never undo that continuity. I returned to my friends knowing that our fantasy's deepest law had no forgotten exception.";
                default:
                    return null;
            }
        }

        public static string ResidentFriends(GeneratorDefinition creation)
        {
            if (creation.resident_friends == null || creation.resident_friends.Count == 0)
                return creation.fairy;
            return string.Join(" & ", creation.resident_friends.Select(friend => friend.name));
        }

        private static string Emotion(GeneratorDefinition creation)
        {
            if (creation == null) return "relaxed";
            if (creation.id == "emotion_sorrow" || creation.id == "emotion_fear") return "sad";
            if (creation.id == "emotion_anger") return "angry";
            if (creation.era == "Universal Emotions" || creation.era == "Worlds and Spirit") return "happy";
            return "surprised";
        }

        private static string Participation(GeneratorDefinition creation)
        {
            if (creation == null) return "I stayed beside our fragile beginning and gave it my care. I was not its only author. Every companion who joined us changed what our creation could become.";
            string companion = creation.fairy;
            if (creation.era == "Human Life and Soul Resonance")
                return "I helped " + companion + " make room for these different stories without asking anyone to believe them. In our imagined creation, I guard coexistence, not escape: an earthly life still needs food, rest, friendship, and care.";
            if (creation.era == "Universal Emotions")
                return "I sat with " + companion + " and listened before offering my light. I helped make a place for this feeling without forcing it on anyone. Being a goddess did not spare me from learning how to care.";
            switch (creation.era)
            {
                case "Five Elements":
                    return "I helped " + companion + " shape this domain, then tended its connections to the others. I learned that creation needs boundaries as well as generosity: each element must keep its nature while making room for its neighbours.";
                case "Time":
                    return "I waited beside " + companion + " through those first moments. I helped give change a rhythm, without insisting that every world must follow the same clock. For the first time, I could remember a before and welcome an after.";
                case "Space":
                    return "I reached into the emptiness with " + companion + " and helped mark a place to meet. I did not push time aside to make room. I wanted distance to make encounters possible, not make our companions lonely.";
                case "Causal Weave":
                    return "I held the threads steady while " + companion + " joined them. My task was to connect without consuming: time and space kept their own domains, and our new weave gave their meetings a place in the story.";
                case "Quantum Foundations":
                    return "I sheltered the delicate patterns beside " + companion + ". In our fantasy, I helped those possibilities find forms that could endure. This is how I remember our imagined creation, not a lesson in real-world physics.";
                case "Matter and Stars":
                    return "I gathered what had scattered and held it near " + companion + ". I could offer a meeting place, but not hurry every bond into being. When the darkness began to shine, I knew our patience had become part of that light.";
                case "Worlds and Spirit":
                    return "I worked beside " + companion + " to make our creation a home, not merely a beautiful shape. I tended its fragile beginnings and left space for lives that would never belong to me.";
                default:
                    return !string.IsNullOrEmpty(creation.dimension)
                        ? "I helped " + companion + " establish " + creation.name + " and kept its connections open. I made room for it alongside our existing worlds, without asking those worlds to surrender their own laws or identities."
                        : "I welcomed " + companion + " and helped nurture " + creation.name + ". My part was to offer protection and a place among our earlier creations, then listen as my companion shaped something I could not have made alone.";
            }
        }

        private static void Add(List<GenesisStoryPage> pages, string text, string mood, string speaker = "Zenda", string speakerIconId = null)
        {
            string remaining = text.Trim();
            // Keep complete passages until translation; pagination happens afterwards.
            if (remaining.Length > 0) pages.Add(new GenesisStoryPage(remaining, mood, speaker, speakerIconId));
        }

        private static void Paginate(List<GenesisStoryPage> pages, string text, string mood, string speaker, string speakerIconId)
        {
            string remaining = text.Trim();
            while (remaining.Length > PageLength)
            {
                int split = remaining.LastIndexOf(' ', PageLength);
                if (split < 1) split = PageLength;
                pages.Add(new GenesisStoryPage(remaining.Substring(0, split), mood, speaker, speakerIconId));
                remaining = remaining.Substring(split).TrimStart();
            }
            if (remaining.Length > 0) pages.Add(new GenesisStoryPage(remaining, mood, speaker, speakerIconId));
        }
    }
}