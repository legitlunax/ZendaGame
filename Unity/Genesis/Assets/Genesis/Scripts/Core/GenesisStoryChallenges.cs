namespace Genesis
{
    // Shared by the conversation reader and gallery so neither tells a different outcome.
    public static class GenesisStoryChallenges
    {
        public static string[] Build(GeneratorDefinition creation)
        {
            string key = creation == null ? "first_light" : creation.id;
            switch (key)
            {
                case "first_light": return Scene(
                    "I tried to light the silence, but there was nowhere for light to fall. Each time I demanded a whole world, my hands closed on nothing. I had mistaken the destination for the first step.",
                    "You did not need a finished world to begin, Zenda. You sheltered one possibility without claiming it had already become real. That left room for the next choice instead of requiring creation to create itself.",
                    "I opened my hands again. The spark remained a promise, not a world pretending to have a past. With that distinction intact, I could nurture our beginning without inventing an earlier creator to explain it.");
                case "hope_fluctuation": return Scene(
                    "When I reached for one spark, the others dimmed. I tried holding them all in one shape, but then none could change. Protecting every possibility had become a way of preventing any beginning.",
                    "We separated shelter from selection, Zenda. You kept the unchosen sparks safe while I tended one opening. A choice could become real without declaring every other hope impossible.",
                    "I watched that opening endure while its neighbours still shimmered. We could continue creating: choosing a path no longer meant destroying the possibility of another.");
                case "first_intention": return Scene(
                    "I gave our sparks a direction, then pulled them back whenever another direction called. Nothing could begin while I treated every new wish as an order to undo the last.",
                    "We chose one small task, Zenda, and agreed to finish it before reconsidering. An intention could guide our work without becoming a decree that no one could question.",
                    "I held to that first task, then invited another choice. We had resolved our false choice between endless hesitation and an unchangeable command; creation could move and still learn.");
                case "primordial_breath": return Scene(
                    "My first breath scattered the sparks. I stopped breathing to protect them, and our beginning became still. Neither force nor perfect stillness could carry it onward.",
                    "We tried shorter breaths, Zenda, and left quiet intervals between them. I gathered the scattered sparks while you softened the next ripple; we changed the rhythm rather than blaming what was fragile.",
                    "I breathed again and saw the sparks travel without going out. Movement and shelter could share a rhythm. We followed that gentler current into our next work.");
                case "yin": return Scene(
                    "I closed the sanctuary against every disturbance. Soon even a friend's answer could not enter. A refuge meant to receive had become a sealed room.",
                    "We made a threshold instead of a wall, Zenda. Rest could decline an intrusion and still welcome an invitation; receiving did not mean having no choice.",
                    "I called softly from outside and waited. The sanctuary answered in its own time. We kept its quiet without isolating it, then continued tending a place where others could rest.");
                case "yang": return Scene(
                    "I poured radiance into every corner, afraid that any shadow meant failure. The sanctuary's quiet disappeared beneath my enthusiasm. Our gift had begun to crowd out its welcome.",
                    "We gave the light a hearth and an edge, Zenda. It could offer warmth without demanding that every neighbour shine. I asked where it was wanted before carrying it farther.",
                    "I watched brightness meet a resting place without entering uninvited. Expression no longer required the absence of stillness, so we could bring warmth onward without burning away our earlier work.");
                case "yin_yang": return Scene(
                    "I tried to end their disagreement by giving stillness and radiance the same voice. The weave went slack: neither could answer if there was no difference left between them.",
                    "We stopped asking opposites to become identical, Zenda. We gave each a turn to offer and receive, with a boundary neither could cross by force.",
                    "I held the meeting place while both answered differently. Their exchange endured. We had not proved that opposites were the same; we had built a relationship in which difference could sustain creation.");
                case "element_fire": return Scene(
                    "My first hearth went dark when I sealed it too tightly. Opening it without care sent heat toward our neighbour's growing roots. I could not solve one failure by moving it next door.",
                    "We rebuilt the hearth with a tended opening, Zenda, and placed a clear boundary around its warmth. I watched the flame while you checked the roots; either of us could call for a pause.",
                    "I saw the hearth stay alight and the roots remain sheltered. We had given fire room to be fire without giving it every room. Then we carried that lesson into the next dwelling.");
                case "independent_clocks": return Scene(
                    "Two envoys called their meeting now, but their clocks marked different orders of events. I tried setting both clocks together and erased the histories that made their appointments meaningful.",
                    "We withdrew the shared clock, Zenda. Each envoy kept a local sequence, and both acknowledged which message they had received before answering. Correspondence needed an agreed exchange, not a universal now.",
                    "I watched each reply follow its own received message. Neither envoy had to arrive before leaving home. We continued the confluence with local clocks intact and no hidden master clock.");
                case "beyond_distance": return Scene(
                    "I drew a road between universes that did not share space. Its first milestone had no meaning at the far end. Calling the road infinitely long only disguised that we had no common distance to measure.",
                    "We put away the road, Zenda. We agreed on which patterns each world could recognise locally, without claiming that a body crossed a space that did not exist between them.",
                    "I received an answer without moving either world onto our map. Their relationship could grow without a fictional shortcut pretending to be a shared geography.");
                case "lawful_dissonance": return Scene(
                    "A pattern stable in one universe broke apart under another's laws. I first tried declaring one law correct everywhere. That would have saved our pattern by erasing a world.",
                    "We kept the pattern at home and exchanged a description, Zenda. Each world made its own lawful response; when a description had no local meaning, we marked that limit instead of forcing an answer.",
                    "I saw two different responses remain sound in their own worlds. No object had to obey incompatible rules in the same domain. We continued learning without making agreement a condition of existence.");
                case "uncollapsed_possibility": return Scene(
                    "I reached into the garden to count every possible blossom as though it were already a single settled flower. In naming one outcome, I began closing the others we had promised to shelter.",
                    "We marked what was possible separately from what had been chosen, Zenda. We could tend the garden's conditions without requiring every branch to settle into our preferred account.",
                    "I left the unchosen paths open and recorded only the choice we actually made. Possibility was not a disguised certainty. We could act in one place without claiming to have finished every other story.");
                case "all_knowing": return Scene(
                    "I answered a friend's question before she finished asking. She fell quiet. I had confused understanding a possibility with permission to decide which words she would speak.",
                    "We made room for an answer freely given, Zenda. Knowing what could be said was not the same as saying it for someone. An unsettled choice stayed unsettled in our account.",
                    "I waited, and her answer changed our work. Knowledge no longer swallowed participation. We continued with more understanding, not fewer voices.");
                case "omnipotence": return Scene(
                    "I tried to promise both that everyone chose freely and that no one could ever refuse my plan. My promise contradicted itself before we had built anything.",
                    "We withdrew the demand for obedience, Zenda. Power could offer support and protect a boundary, but a freely given yes had to leave room for no. We changed the plan instead of changing our friends.",
                    "I heard a refusal and made another invitation. Our work continued with consent intact. We had not solved a contradiction by force; we had stopped making the contradictory demand.");
                case "soul_dawn": return Scene(
                    "At the garden of farewells, I feared that leaving one form behind meant losing the being who had lived in it. Building a doorway was not enough if its traveller vanished at the threshold.",
                    "We distinguished an ending of form from an erasure of life, Zenda. Morthelia tended the farewell; Life held the same living being safely through it. Neither a name on a stone nor a replacement would have been enough.",
                    "I stayed while the traveller continued, still themselves, with memory and choice intact. The garden could honour change without claiming authority to erase anyone. We returned to tending its paths.");
            }
            switch (creation == null ? "" : creation.era)
            {
                case "Five Elements": return Scene(
                    "At " + creation.name + ", our first boundary kept everything safe by allowing nothing through. When I removed it, neighbouring elements overwhelmed the work. A sealed home and an unguarded one both failed us.",
                    "We made a small meeting place at the edge, Zenda. We tried one exchange, checked both sides, and narrowed it when either neighbour lost its balance. Cooperation did not require surrendering a domain.",
                    "I watched the exchange hold before opening another. Each element kept its nature, and their connection stopped consuming what it joined. We continued building from that tested boundary.");
                case "Time": return Scene(
                    "At " + creation.name + ", I tried making every change happen together. A seed's opening and its later growth lost their order. Then I froze the work to protect it, leaving no moment in which it could grow.",
                    "We gave each change a local before and after, Zenda. We marked what had happened before allowing the next step, without requiring every distant world to use our clock.",
                    "I watched one change lead to the next without rewriting the first. Time could carry a history rather than flatten it. We resumed our work one acknowledged moment at a time.");
                case "Space": return Scene(
                    "At " + creation.name + ", I marked two meeting places with the same sign. Our friends followed it and missed one another. Removing distance would have removed their homes as well as their separation.",
                    "We gave the places distinct landmarks, Zenda, then agreed on a route and a return sign. Finding each other needed a relationship between places, not a claim that every place was one.",
                    "I followed the signs there and back. Our homes stayed distinct and our meeting became possible. We continued laying paths without folding away the room others needed.");
                case "Causal Weave": return Scene(
                    "At " + creation.name + ", I tied a reply to the message it was meant to cause. The thread demanded an answer before anyone had asked. Pulling it tighter only made the knot harder to see.",
                    "We untied the loop, Zenda, and traced each event to a step already made. A possible reply could guide a plan, but it could not serve as evidence that its own question had happened.",
                    "I sent the question, waited, and received a new answer. No thread had to supply its own origin. With the missing step restored, we could extend the weave without carrying that knot onward.");
                case "Quantum Foundations": return Scene(
                    "At " + creation.name + ", I treated every possible pattern as an already finished structure. Our next layer had nothing settled to rest on. My attempt to hurry certainty left the work less secure.",
                    "We separated the patterns we were exploring from the form we had established, Zenda. We tested a small foundation before adding weight, and kept uncertain descriptions marked as uncertain.",
                    "I watched the new layer hold on the tested form. In our fantasy, possibility and an established event no longer made incompatible claims about the same step. We continued carefully; this was our imagined creation, not real-world physics.");
                case "Matter and Stars": return Scene(
                    "At " + creation.name + ", I gathered everything into one brilliant centre. The outer work thinned and the centre would not hold. More force repeated the failure instead of repairing it.",
                    "We loosened our grip, Zenda, and tended a smaller gathering with room around it. We checked what remained outside as carefully as what drew together; no new light was worth an emptied neighbour.",
                    "I saw a lasting pattern where the scattered work had been. Our story did not need light to feed its own beginning: we built from what was already there, then continued nurturing the next gathering.");
                case "Worlds and Spirit": return Scene(
                    "At " + creation.name + ", our first design looked complete until we followed its connections. One region received what it needed while another was left without. I had mistaken a beautiful part for a working home.",
                    "We traced what each region received and returned, Zenda. We changed the route that stranded its neighbour, then watched a whole cycle before adding more. Life's shelter was never an excuse to neglect its home.",
                    "I watched the cycle return without leaving the first region empty. Our remedy no longer moved the difficulty out of sight. We continued tending the world, with care for what our next addition would touch.");
                case "Living Universe": return Scene(
                    "At " + creation.name + ", I joined two distant patterns without agreeing on a return. The first exchange worked, but the next piled everything on one side. A connection that only took could not sustain our universe.",
                    "We limited the exchange, Zenda, and checked a full outward-and-return sequence. Where the route could not carry a return, we stopped calling it a balanced passage and rebuilt the missing step.",
                    "I watched both ends remain whole through the next exchange. Connection no longer meant an unexplained source or an unseen loss. We could continue the larger pattern without hiding its cost in another place.");
                case "Universe and Beyond": return Scene(
                    "At " + creation.name + ", two worlds gave the same event different histories. I tried combining their records and made one person both leave and never leave the same home.",
                    "We kept each history with its own world, Zenda. A branch was not an instruction to overwrite its neighbour. When we spoke across them, we named whose history we meant before drawing a conclusion.",
                    "I read both accounts again; neither contradicted itself. Difference between worlds was no longer a contradiction inside one. We could continue exploring without borrowing another world's past.");
                case "Omnielle's Universe Confluence": return Scene(
                    "At " + creation.name + ", our first account treated separate universes as rooms under one sky. It quietly gave them a shared clock and space, undoing the independence we meant to protect.",
                    "We removed the imagined corridor, Zenda. Each universe kept its own laws and history. We compared local patterns without claiming that their agreement required travel, a common now, or identical laws.",
                    "I checked both accounts after the comparison. Neither needed an event outside its own lawful history. The confluence could deepen without erasing the differences that made its many worlds possible.");
                case "Three Thousand Dao": return Scene(
                    "At " + creation.name + ", two paths used the same word for different practices. I declared them identical, and each traveller was asked to follow instructions belonging to the other.",
                    "We asked each traveller what the word meant on her own path, Zenda. Where the practices differed, we marked the distinction and offered a meeting rather than an enforced merger.",
                    "I followed each set of instructions in its own setting. Both paths remained walkable. Concord no longer required incompatible directions to be obeyed as one, and our garden could keep growing.");
                case "Reality": return Scene(
                    "I described a possible world so vividly that I began treating the description as an event already lived. Our record demanded consequences from a choice no one had made.",
                    "We separated imagining, choosing, and remembering, Zenda. A description could invite a creation, but we recorded it as lived only after its inhabitants had a history of their own.",
                    "I corrected the record before building onward. What might be no longer pretended to be what had happened. Reality could welcome new creation without borrowing a false past.");
                case "Life Remembers": return Scene(
                    "At " + creation.name + ", I made a careful account of a friend's memories and nearly called the account the friend herself. A perfect description would still leave unanswered who continued living.",
                    "We kept witness and identity distinct, Zenda. Life sheltered the same being, not merely information about her. Her private thoughts remained hers; remembering someone did not give us ownership of them.",
                    "I listened when she continued her own unfinished thought. Our record supported her story instead of replacing its teller. We could keep remembering together without turning a living friend into an archive.");
                case "Divine Recognition": return Scene(
                    "At " + creation.name + ", I mistook the reach of our awareness for an invitation to enter every private moment. My friend stepped back, and I saw that being present everywhere could become leaving no room for anyone.",
                    "We made availability different from intrusion, Zenda. You offered help and waited for an invitation. A boundary did not make care absent; it made receiving that care a choice.",
                    "I waited outside and was welcomed in. Presence and privacy no longer cancelled one another. We continued learning with our friends, not through taking their place.");
                case "Living Divinity": return Scene(
                    "At " + creation.name + ", I wanted to finish every difficulty for everyone. My first plan left our friends with no decisions of their own. A creation called complete had no room to live.",
                    "We distinguished safety from a life already decided, Zenda. Life held every being safely themselves, while each kept the freedom to learn, disagree, and begin something we had not planned.",
                    "I put down the finished plan and joined our friends' next task. Care did not require a motionless perfect ending. We continued creating without making freedom an exception to Life's shelter.");
                case "Dimensional Life: Angels": return Realm(creation, "I offered protection before asking what help was wanted. Our first sanctuary began to feel like a place no one was allowed to leave.", "We made protection an offer, not a command, and gave every resident a way to decline or leave without losing our friendship.");
                case "Dimensional Life: Demons": return Realm(creation, "I mistook fierce disagreement for a threat and tried quieting every voice. The resulting peace concealed the needs we had not heard.", "We let each resident speak, agreed on boundaries against coercion, and changed the arrangement when someone refused it. Difference was not a licence to harm, nor a reason to erase a people.");
                case "Dimensional Life: Elves and Fairies": return Realm(creation, "Our first path crossed a growing home. I moved it without asking and blocked another dwelling instead. My remedy had only passed the obstruction onward.", "We walked the route with its residents, left room for growth, and tested the return path too. We rebuilt the crossing instead of asking a neighbour to disappear.");
                case "Dimensional Life: Gods": return Realm(creation, "Two keepers promised authority over the same threshold. Each promise was harmless alone, but together they demanded obedience to conflicting commands.", "We limited each promise to an agreed responsibility and gave shared decisions a council. No title could make one person's consent count as everyone else's.");
                case "Dimensional Life: Shadows": return Realm(creation, "I brought light to every corner, believing welcome meant making everything visible. A resident asked where she could keep a private thought, and I had no answer.", "We made a sheltered place and asked before illuminating it. Darkness was not automatically danger, and privacy was not a flaw our light had to remove.");
                case "Concord of Living Realms": return Scene(
                    "At " + creation.name + ", an envoy's welcome was mistaken for permission to change her whole realm. Our first bridge carried a promise larger than anyone had agreed to give.",
                    "We narrowed the agreement, Zenda. Each realm named what its envoy could promise, and each traveller accepted the destination's boundaries. A bridge connected homes; it did not transfer ownership of them.",
                    "I crossed as a guest and returned with the agreement intact. Neither home had to become the other. We reopened the exchange and kept a way to pause it if its terms stopped serving either friend.");
                case "Universal Emotions": return Scene(
                    "At " + creation.name + ", I tried making the whole gathering share one feeling. A friend who felt differently fell silent. Our harmony had become a demand to hide part of a life.",
                    "We listened separately, Zenda. We let a feeling be heard without making it an instruction everyone must follow. Where an action hurt someone, we changed the action rather than denying the feeling.",
                    "I heard different answers without the gathering breaking apart. Shared care did not require identical hearts. We continued together with room for both an honest feeling and another person's boundary.");
                case "Human Life and Soul Resonance": return Scene(
                    "At " + creation.name + ", I almost treated a glimpse of our imagined realms as an answer to an earthly need. A beautiful account had not provided food, rest, or a friend's practical help.",
                    "We returned to the need in front of us, Zenda. We tended the daily task and left each person free to understand the story differently. No vision could demand belief or replace ordinary care.",
                    "I finished the practical work before telling the next memory. Our fantasy could accompany a life without claiming to overrule it. We continued with both imagination and our earthly responsibilities in view.");
                default: return Scene(
                    "I hurried to call our work complete before asking who could continue it. When I stepped away, the next task had no one free to choose how it should be done.",
                    "We invited our friends to change the plan, Zenda. We made each promise clear and checked that accepting one did not require breaking another.",
                    "I watched a friend carry the work forward in her own way. Our beginning no longer depended on my deciding every ending, and we could continue creating together.");
            }
        }

        private static string[] Scene(string obstacle, string response, string continuation)
        { return new[] { obstacle, response, continuation }; }

        private static string[] Realm(GeneratorDefinition creation, string obstacle, string response)
        {
            return Scene("At " + creation.name + ", " + obstacle,
                "Zenda, " + response,
                "I returned to find the residents using the new agreement without surrendering their home or their choices. Our welcome no longer contradicted their freedom. We continued building together, checking each addition with those who would live with it.");
        }
    }
}