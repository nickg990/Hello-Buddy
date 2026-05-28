**AI Tools Overview — Key Functions, Benefits and Weaknesses**

*Concise comparative tables prepared from the discussion. Snapshot as of
May 2026; individual tools and feature availability change quickly.*

# General AI assistants

| **Tool**          | **Main function**                                                                               | **Strongest benefits**                                                                                                                       | **Weaknesses / cautions**                                                                                                                            |
|-------------------|-------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| ChatGPT           | General reasoning, writing, analysis, coding, document work, image generation, research, agents | Best all-rounder; good for drafting, structured analysis, iterative work, files, images, coding, memory/projects depending on plan.          | Can still overstate completeness; web/product/visual search can be weaker than direct human browsing; needs tight instructions for high-stakes work. |
| Claude            | Long-form reasoning, writing, coding, document review, large context work                       | Often strong at prose, careful drafting, code review, and large documents.                                                                   | Can be cautious or verbose; weaker ecosystem/tool integration than Microsoft/Google/OpenAI in some workflows.                                        |
| Gemini            | Google-integrated assistant, multimodal reasoning, search-adjacent tasks, Workspace help        | Strong fit for Google users: Gmail, Docs, Drive, Android, Search and Workspace integration.                                                  | Can be uneven; product naming and rollout patterns can be confusing; privacy/settings need attention in Workspace/Gmail contexts.                    |
| Microsoft Copilot | Work assistant inside Microsoft 365, Windows, Teams, Outlook, Word, Excel, PowerPoint           | Best fit for Microsoft-heavy workplaces; designed to use work context, chat, search, agents, notebooks, file prompts and Microsoft 365 data. | Less attractive outside Microsoft ecosystem; quality depends heavily on tenant setup, permissions and document hygiene.                              |
| Perplexity        | AI answer engine / research assistant                                                           | Strong for web-grounded answers, source comparison, quick research, and multi-model comparison.                                              | Not a substitute for primary-source verification; can summarise sources that are themselves poor or incomplete.                                      |
| Grok              | X/xAI assistant with real-time/X-connected flavour                                              | Strongest appeal is real-time/current conversation around X and a more informal/witty style.                                                 | More variable tone and trust profile; less suitable where sober evidential discipline is needed.                                                     |
| Mistral Le Chat   | European AI assistant for chat, research, reasoning, agents, multilingual use                   | Fast, privacy/control-oriented positioning; customisable, agent-capable, with research, creation, voice and enterprise options.              | Smaller ecosystem than OpenAI/Google/Microsoft; may be less familiar to casual users.                                                                |

# Specialist tools by task

| **Need**                            | **Better tools**                                                               | **Why they matter**                                                        | **Main limitation**                                                                 |
|-------------------------------------|--------------------------------------------------------------------------------|----------------------------------------------------------------------------|-------------------------------------------------------------------------------------|
| Fast web research                   | Perplexity, ChatGPT deep research, Gemini, Copilot                             | Pulls together sources faster than manual searching.                       | Must verify source quality; not all “citations” are equally useful.                 |
| Writing / editing / drafting        | ChatGPT, Claude, Gemini, Copilot                                               | Good for structure, tone, redrafting, summarising.                         | Can smooth away meaning unless tightly controlled.                                  |
| Large document review               | Claude, ChatGPT, Gemini, Copilot                                               | Useful for long PDFs, contracts, reports, academic work.                   | Risk of omissions; ask for evidence-linked outputs.                                 |
| Coding                              | ChatGPT/Codex, Claude, Copilot, Perplexity Computer                            | Strong at debugging, scaffolding, explaining, code review.                 | Version/dependency mismatches; must test actual code.                               |
| Microsoft work documents            | Microsoft 365 Copilot                                                          | Best placed for Word, Excel, Outlook, Teams context.                       | Only as good as access permissions and file organisation.                           |
| Google ecosystem                    | Gemini                                                                         | Better fit for Gmail, Docs, Drive, Android and Search-connected workflows. | Product behaviour may vary by account, region and settings.                         |
| Image generation                    | ChatGPT image tools, Midjourney, Adobe Firefly, Ideogram, Leonardo             | Different strengths: realism, design, text-in-image, commercial workflows. | Prompting and rights/licensing vary; text in images still needs checking.           |
| Video generation                    | Runway, Pika, Kling, Luma, Sora-type tools where available                     | Short generated clips, motion concepts, visualisation.                     | Still unreliable for continuity, realism, hands/faces, exact instruction following. |
| Meeting transcription / notes       | Otter, Fireflies, Teams/Zoom AI, Google Meet AI                                | Saves time on meetings and interviews.                                     | Accuracy varies with audio quality; sensitive data concerns.                        |
| Search within personal/company data | Copilot, Gemini Workspace, ChatGPT connectors/projects, enterprise RAG tools   | Can query internal documents and emails.                                   | Risk of permissions errors, stale files, incomplete indexes.                        |
| Academic/literature search          | Elicit, Consensus, Semantic Scholar, Perplexity, Google Scholar + AI summaries | Useful for paper discovery and evidence mapping.                           | Needs manual verification; summaries can miss methodological weaknesses.            |
| Automation / agents                 | ChatGPT agents, Copilot agents, Perplexity Computer, Zapier AI, Make AI        | Can perform multi-step workflows.                                          | Agentic tools still fail in brittle ways; supervision needed.                       |

# Practical selection guide

| **User situation**                                             | **Best starting point**                                       |
|----------------------------------------------------------------|---------------------------------------------------------------|
| “I want one AI for most things”                                | ChatGPT                                                       |
| “I write/review long documents and want careful prose”         | Claude or ChatGPT                                             |
| “My work is all Microsoft 365”                                 | Microsoft Copilot                                             |
| “My life/work is Google Workspace and Android”                 | Gemini                                                        |
| “I want sourced answers and quick web research”                | Perplexity                                                    |
| “I want current/X-flavoured commentary”                        | Grok                                                          |
| “I care about European/provider diversity/privacy positioning” | Mistral Le Chat                                               |
| “I mostly need code help”                                      | ChatGPT/Codex, Claude, Copilot                                |
| “I need images/design”                                         | ChatGPT image generation, Midjourney, Adobe Firefly, Ideogram |
| “I need meeting notes”                                         | Otter, Fireflies, Teams/Zoom/Google native AI                 |

# Biggest general weaknesses across AI tools

| **Weakness**          | **What it looks like in practice**                                                                                                    |
|-----------------------|---------------------------------------------------------------------------------------------------------------------------------------|
| False completeness    | The answer sounds finished, but important sources/options were missed.                                                                |
| Near-match problem    | It returns things that almost fit but fail a critical criterion.                                                                      |
| Source laundering     | Weak sources get summarised into authoritative-sounding prose.                                                                        |
| Context loss          | Long workflows drift unless the task is tightly structured.                                                                           |
| Visual/detail failure | Product variants, images, tables, diagrams, and small visual distinctions can be mishandled.                                          |
| Over-smoothing        | Drafting tools may soften or rebalance wording in ways that alter meaning.                                                            |
| Tool mismatch         | The AI proceeds when a human search, spreadsheet, database, solicitor, GP, specialist software, or direct inspection would be better. |
| Agent brittleness     | Multi-step web tasks can fail silently or get stuck in loops.                                                                         |

# Suggested roles for this user’s work style

| **Role**                     | **Best use**                                                                                                                                                                    |
|------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Thinking partner             | ChatGPT / Claude                                                                                                                                                                |
| Evidence organiser           | ChatGPT / Claude / Perplexity                                                                                                                                                   |
| Web research scout           | Perplexity / ChatGPT deep research / Gemini                                                                                                                                     |
| Workplace assistant          | Copilot if Microsoft-heavy; Gemini if Google-heavy                                                                                                                              |
| Visual creator               | ChatGPT image tools / Midjourney / Firefly                                                                                                                                      |
| Automation helper            | ChatGPT agents / Copilot agents / Zapier/Make                                                                                                                                   |
| Not suitable as primary tool | Visually constrained shopping searches, forensic audio/video interpretation, legal/medical conclusions without source material, anything where hidden details decide the answer |

**Operational rule: before using any AI tool, ask whether the task
depends on text reasoning, visual scanning, live availability, private
data access, specialist judgement, or exact source verification. That
determines whether AI is the right tool, a helper, or a nuisance.**
