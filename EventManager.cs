using System;
using System.Collections.Generic;
using System.Linq;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles corporate event definitions and generation.
    /// </summary>
    public class EventManager
    {
        private readonly Logger _logger;
        private readonly Random _random = new Random();
        private readonly Dictionary<string, CorporateEvent> _eventLibrary;

        public EventManager(Logger logger)
        {
            _logger = logger;
            _eventLibrary = BuildEventLibrary();
        }

        public CorporateEvent GetEventById(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return null;
            }

            return _eventLibrary.ContainsKey(eventId) ? CloneEvent(_eventLibrary[eventId]) : null;
        }

        public List<CorporateEvent> GenerateDailyEvents(CEOStats stats, int day)
        {
            var events = new List<CorporateEvent>();
            var count = _random.Next(2, 5);
            var weightedCategories = GetWeightedCategories(stats);

            for (var i = 0; i < count; i++)
            {
                var category = weightedCategories[_random.Next(weightedCategories.Count)];
                var candidates = _eventLibrary.Values.Where(e => e.Category == category).ToList();
                if (candidates.Count == 0)
                {
                    candidates = _eventLibrary.Values.ToList();
                }

                var selected = candidates[_random.Next(candidates.Count)];
                events.Add(CloneEvent(selected));
            }

            _logger.Info($"Generated {events.Count} events for day {day}.");
            return events;
        }

        private List<string> GetWeightedCategories(CEOStats stats)
        {
            var categories = new List<string>();
            categories.AddRange(Enumerable.Repeat("Acquisition", 3));
            categories.AddRange(Enumerable.Repeat("PR", 3));
            categories.AddRange(Enumerable.Repeat("Legal", 3));
            categories.AddRange(Enumerable.Repeat("Labor", 3));
            categories.AddRange(Enumerable.Repeat("Board", 3));

            if (stats.RiskLevel > 60)
            {
                categories.AddRange(Enumerable.Repeat("Legal", 4));
            }

            if (stats.PublicReputation < 40)
            {
                categories.AddRange(Enumerable.Repeat("PR", 4));
            }

            if (stats.EmployeeMorale < 40)
            {
                categories.AddRange(Enumerable.Repeat("Labor", 4));
            }

            if (stats.BoardApproval < 40)
            {
                categories.AddRange(Enumerable.Repeat("Board", 4));
            }

            return categories;
        }

        private CorporateEvent CloneEvent(CorporateEvent original)
        {
            return new CorporateEvent
            {
                Id = original.Id,
                Title = original.Title,
                Category = original.Category,
                Severity = original.Severity,
                Description = original.Description,
                Choices = original.Choices.Select(choice => new EventChoice
                {
                    Label = choice.Label,
                    OutcomeText = choice.OutcomeText,
                    StatDeltas = new StatDeltas
                    {
                        Power = choice.StatDeltas.Power,
                        PublicReputation = choice.StatDeltas.PublicReputation,
                        BoardApproval = choice.StatDeltas.BoardApproval,
                        EmployeeMorale = choice.StatDeltas.EmployeeMorale,
                        RiskLevel = choice.StatDeltas.RiskLevel
                    },
                    CashDelta = choice.CashDelta,
                    StockDelta = choice.StockDelta,
                    Flags = new List<string>(choice.Flags),
                    DelayedEventId = choice.DelayedEventId
                }).ToList()
            };
        }

        private Dictionary<string, CorporateEvent> BuildEventLibrary()
        {
            var events = new List<CorporateEvent>
            {
                new CorporateEvent
                {
                    Id = "acq_1",
                    Title = "Midwest Logistics Buyout",
                    Category = "Acquisition",
                    Severity = 3,
                    Description = "A regional logistics firm is open to acquisition at a 15% premium. Analysts say it could cut delivery costs within 6 months.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Approve the buyout",
                            OutcomeText = "You greenlight the deal and the market responds favorably.",
                            StatDeltas = new StatDeltas { BoardApproval = 3, Power = 2, RiskLevel = 2 },
                            CashDelta = -45000,
                            StockDelta = 3.5f
                        },
                        new EventChoice
                        {
                            Label = "Negotiate a lower premium",
                            OutcomeText = "Negotiations drag out but protect capital.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, EmployeeMorale = 1 },
                            CashDelta = -25000,
                            StockDelta = 1.5f
                        },
                        new EventChoice
                        {
                            Label = "Pass for now",
                            OutcomeText = "You hold back, missing a growth opportunity.",
                            StatDeltas = new StatDeltas { BoardApproval = -2, Power = -1 },
                            CashDelta = 0,
                            StockDelta = -1.0f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "acq_2",
                    Title = "Fintech Partnership Proposal",
                    Category = "Acquisition",
                    Severity = 2,
                    Description = "A fintech startup wants a minority investment to access your client base. They want 20% for $30M equivalent.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Invest and integrate",
                            OutcomeText = "You secure a seat at the table and a tech edge.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, Power = 1 },
                            CashDelta = -30000,
                            StockDelta = 2.0f
                        },
                        new EventChoice
                        {
                            Label = "Offer a pilot program",
                            OutcomeText = "The startup agrees to a limited pilot with optional buyout.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, EmployeeMorale = 1 },
                            CashDelta = -8000,
                            StockDelta = 0.8f
                        },
                        new EventChoice
                        {
                            Label = "Decline the offer",
                            OutcomeText = "You pass, citing valuation concerns.",
                            StatDeltas = new StatDeltas { BoardApproval = -1 },
                            CashDelta = 0,
                            StockDelta = -0.5f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "acq_3",
                    Title = "Hostile Bid Rumors",
                    Category = "Acquisition",
                    Severity = 4,
                    Description = "Rumors claim a rival is preparing a hostile bid for a key supplier you rely on.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Preempt with a friendly offer",
                            OutcomeText = "You move fast and secure a friendly agreement.",
                            StatDeltas = new StatDeltas { BoardApproval = 3, Power = 2, RiskLevel = 1 },
                            CashDelta = -60000,
                            StockDelta = 3.0f
                        },
                        new EventChoice
                        {
                            Label = "Hold position",
                            OutcomeText = "You decide not to enter a bidding war.",
                            StatDeltas = new StatDeltas { BoardApproval = -2, Power = -1 },
                            CashDelta = 0,
                            StockDelta = -2.5f
                        },
                        new EventChoice
                        {
                            Label = "Seek a joint venture",
                            OutcomeText = "A JV reduces risk but adds complexity.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = 1 },
                            CashDelta = -25000,
                            StockDelta = 1.0f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "acq_4",
                    Title = "Rural Healthcare Assets",
                    Category = "Acquisition",
                    Severity = 2,
                    Description = "A portfolio of rural clinics is available below market value due to cash flow issues.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Acquire and modernize",
                            OutcomeText = "You announce a modernization plan.",
                            StatDeltas = new StatDeltas { PublicReputation = 2, BoardApproval = 1 },
                            CashDelta = -35000,
                            StockDelta = 1.2f
                        },
                        new EventChoice
                        {
                            Label = "Acquire and cut costs",
                            OutcomeText = "The market approves but morale dips.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, EmployeeMorale = -2, RiskLevel = 1 },
                            CashDelta = -28000,
                            StockDelta = 2.2f,
                            Flags = new List<string> { "cost_cut" }
                        },
                        new EventChoice
                        {
                            Label = "Pass on the assets",
                            OutcomeText = "You avoid a turnaround headache.",
                            StatDeltas = new StatDeltas { BoardApproval = -1 },
                            CashDelta = 0,
                            StockDelta = -0.7f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "acq_5",
                    Title = "European Expansion",
                    Category = "Acquisition",
                    Severity = 5,
                    Description = "An opportunity to acquire a European competitor would add 12% market share but exposes regulatory risk.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Push the acquisition",
                            OutcomeText = "You push aggressively into Europe.",
                            StatDeltas = new StatDeltas { Power = 3, BoardApproval = 2, RiskLevel = 3 },
                            CashDelta = -90000,
                            StockDelta = 4.5f,
                            DelayedEventId = "legal_4"
                        },
                        new EventChoice
                        {
                            Label = "Stage it in phases",
                            OutcomeText = "A phased plan keeps regulators calmer.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = 1 },
                            CashDelta = -60000,
                            StockDelta = 2.0f
                        },
                        new EventChoice
                        {
                            Label = "Wait for next quarter",
                            OutcomeText = "You delay for more due diligence.",
                            StatDeltas = new StatDeltas { BoardApproval = -2 },
                            CashDelta = 0,
                            StockDelta = -1.5f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "pr_1",
                    Title = "Data Privacy Leak",
                    Category = "PR",
                    Severity = 4,
                    Description = "A small data leak hit social media. Journalists are asking for a statement.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Full transparency",
                            OutcomeText = "You own the mistake and announce fixes.",
                            StatDeltas = new StatDeltas { PublicReputation = 3, BoardApproval = 1 },
                            CashDelta = -12000,
                            StockDelta = -0.5f
                        },
                        new EventChoice
                        {
                            Label = "Minimal disclosure",
                            OutcomeText = "You keep the statement short and legal.",
                            StatDeltas = new StatDeltas { PublicReputation = -2, RiskLevel = 3 },
                            CashDelta = -3000,
                            StockDelta = -1.5f,
                            Flags = new List<string> { "coverup" },
                            DelayedEventId = "legal_2"
                        },
                        new EventChoice
                        {
                            Label = "Blame a vendor",
                            OutcomeText = "The blame shift protects the brand but strains morale.",
                            StatDeltas = new StatDeltas { PublicReputation = -1, EmployeeMorale = -2, RiskLevel = 2 },
                            CashDelta = 0,
                            StockDelta = -1.0f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "pr_2",
                    Title = "Executive Interview Request",
                    Category = "PR",
                    Severity = 2,
                    Description = "A major business network wants a live interview about your strategy.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Accept and prepare",
                            OutcomeText = "Your polished answers lift confidence.",
                            StatDeltas = new StatDeltas { PublicReputation = 2, BoardApproval = 1 },
                            CashDelta = -2000,
                            StockDelta = 1.0f
                        },
                        new EventChoice
                        {
                            Label = "Decline",
                            OutcomeText = "The press notes your absence.",
                            StatDeltas = new StatDeltas { PublicReputation = -1 },
                            CashDelta = 0,
                            StockDelta = -0.3f
                        },
                        new EventChoice
                        {
                            Label = "Send the COO",
                            OutcomeText = "The COO handles it but the board wanted you.",
                            StatDeltas = new StatDeltas { BoardApproval = -1, PublicReputation = 1 },
                            CashDelta = 0,
                            StockDelta = 0.3f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "pr_3",
                    Title = "Community Grant Request",
                    Category = "PR",
                    Severity = 1,
                    Description = "Local leaders request a $1M-equivalent grant for workforce training.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Fund the program",
                            OutcomeText = "Positive headlines follow the announcement.",
                            StatDeltas = new StatDeltas { PublicReputation = 3, EmployeeMorale = 1 },
                            CashDelta = -10000,
                            StockDelta = 0.5f
                        },
                        new EventChoice
                        {
                            Label = "Offer a smaller grant",
                            OutcomeText = "A reduced commitment still helps.",
                            StatDeltas = new StatDeltas { PublicReputation = 1 },
                            CashDelta = -4000,
                            StockDelta = 0.2f
                        },
                        new EventChoice
                        {
                            Label = "Decline",
                            OutcomeText = "You cite budget constraints.",
                            StatDeltas = new StatDeltas { PublicReputation = -1 },
                            CashDelta = 0,
                            StockDelta = -0.2f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "pr_4",
                    Title = "Product Safety Concern",
                    Category = "PR",
                    Severity = 3,
                    Description = "A whistleblower alleges a product line has safety issues. Media is preparing a report.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Recall and fix",
                            OutcomeText = "You issue a proactive recall.",
                            StatDeltas = new StatDeltas { PublicReputation = 2, BoardApproval = 1 },
                            CashDelta = -20000,
                            StockDelta = -1.0f
                        },
                        new EventChoice
                        {
                            Label = "Silent investigation",
                            OutcomeText = "You investigate quietly while keeping sales open.",
                            StatDeltas = new StatDeltas { RiskLevel = 3, BoardApproval = 1 },
                            CashDelta = 0,
                            StockDelta = 0.5f,
                            Flags = new List<string> { "coverup" },
                            DelayedEventId = "legal_1"
                        },
                        new EventChoice
                        {
                            Label = "Publicly challenge the claim",
                            OutcomeText = "You deny wrongdoing and prepare for backlash.",
                            StatDeltas = new StatDeltas { PublicReputation = -2, RiskLevel = 2 },
                            CashDelta = -5000,
                            StockDelta = -1.2f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "pr_5",
                    Title = "Viral Employee Story",
                    Category = "PR",
                    Severity = 2,
                    Description = "A viral post highlights one of your employees struggling with benefits.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Improve benefits immediately",
                            OutcomeText = "You announce benefit upgrades.",
                            StatDeltas = new StatDeltas { PublicReputation = 2, EmployeeMorale = 3 },
                            CashDelta = -12000,
                            StockDelta = -0.4f
                        },
                        new EventChoice
                        {
                            Label = "Announce a review",
                            OutcomeText = "You promise a review and short-term assistance.",
                            StatDeltas = new StatDeltas { PublicReputation = 1, EmployeeMorale = 1 },
                            CashDelta = -4000,
                            StockDelta = 0.0f
                        },
                        new EventChoice
                        {
                            Label = "Ignore the story",
                            OutcomeText = "The story lingers and morale dips.",
                            StatDeltas = new StatDeltas { PublicReputation = -2, EmployeeMorale = -2 },
                            CashDelta = 0,
                            StockDelta = -0.8f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "legal_1",
                    Title = "Regulatory Inquiry",
                    Category = "Legal",
                    Severity = 4,
                    Description = "Regulators request documents on product compliance. Deadline is tight.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Cooperate fully",
                            OutcomeText = "You provide the documents and invite inspectors.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = -3 },
                            CashDelta = -6000,
                            StockDelta = 0.5f
                        },
                        new EventChoice
                        {
                            Label = "Slow roll the response",
                            OutcomeText = "You delay, hoping the heat cools down.",
                            StatDeltas = new StatDeltas { RiskLevel = 4, BoardApproval = -1 },
                            CashDelta = -1000,
                            StockDelta = -0.5f,
                            Flags = new List<string> { "coverup" }
                        },
                        new EventChoice
                        {
                            Label = "Hire a top law firm",
                            OutcomeText = "High-powered counsel calms the board.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, RiskLevel = -1 },
                            CashDelta = -15000,
                            StockDelta = 0.4f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "legal_2",
                    Title = "Shareholder Lawsuit",
                    Category = "Legal",
                    Severity = 5,
                    Description = "A shareholder lawsuit alleges you misrepresented performance projections.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Settle quickly",
                            OutcomeText = "You settle to keep it quiet.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = -2 },
                            CashDelta = -25000,
                            StockDelta = -1.0f
                        },
                        new EventChoice
                        {
                            Label = "Fight aggressively",
                            OutcomeText = "You take a hard stance in court.",
                            StatDeltas = new StatDeltas { Power = 1, RiskLevel = 3 },
                            CashDelta = -12000,
                            StockDelta = -0.8f
                        },
                        new EventChoice
                        {
                            Label = "Quiet mediation",
                            OutcomeText = "Mediation buys time and reduces exposure.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, RiskLevel = -1 },
                            CashDelta = -15000,
                            StockDelta = 0.2f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "legal_3",
                    Title = "Compliance Audit",
                    Category = "Legal",
                    Severity = 3,
                    Description = "An internal audit found gaps in compliance reporting.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Fix and disclose",
                            OutcomeText = "You clean up reporting and disclose improvements.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = -2 },
                            CashDelta = -8000,
                            StockDelta = 0.3f
                        },
                        new EventChoice
                        {
                            Label = "Cover it up",
                            OutcomeText = "You bury the report and move on.",
                            StatDeltas = new StatDeltas { RiskLevel = 4, BoardApproval = -2 },
                            CashDelta = 0,
                            StockDelta = -1.0f,
                            Flags = new List<string> { "coverup" },
                            DelayedEventId = "legal_5"
                        },
                        new EventChoice
                        {
                            Label = "Outsource compliance",
                            OutcomeText = "You bring in consultants and new tools.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, RiskLevel = -1 },
                            CashDelta = -12000,
                            StockDelta = 0.6f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "legal_4",
                    Title = "Antitrust Review",
                    Category = "Legal",
                    Severity = 4,
                    Description = "Regulators signal a possible antitrust review after your expansion news.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Offer concessions",
                            OutcomeText = "You offer divestitures to calm regulators.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = -2 },
                            CashDelta = -5000,
                            StockDelta = -0.4f
                        },
                        new EventChoice
                        {
                            Label = "Fight the review",
                            OutcomeText = "You mobilize legal teams and lobbyists.",
                            StatDeltas = new StatDeltas { Power = 2, RiskLevel = 3 },
                            CashDelta = -18000,
                            StockDelta = -0.8f
                        },
                        new EventChoice
                        {
                            Label = "Pause expansion",
                            OutcomeText = "You slow the plan to avoid a showdown.",
                            StatDeltas = new StatDeltas { BoardApproval = -1, RiskLevel = -1 },
                            CashDelta = 0,
                            StockDelta = -0.6f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "legal_5",
                    Title = "Whistleblower Complaint",
                    Category = "Legal",
                    Severity = 5,
                    Description = "A whistleblower filed a complaint about compliance practices.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Launch an internal investigation",
                            OutcomeText = "You cooperate and dedicate resources to it.",
                            StatDeltas = new StatDeltas { RiskLevel = -3, BoardApproval = 1 },
                            CashDelta = -10000,
                            StockDelta = -0.5f
                        },
                        new EventChoice
                        {
                            Label = "Discredit the claim",
                            OutcomeText = "You push back, risking public perception.",
                            StatDeltas = new StatDeltas { RiskLevel = 4, PublicReputation = -2 },
                            CashDelta = -3000,
                            StockDelta = -1.2f,
                            Flags = new List<string> { "coverup" }
                        },
                        new EventChoice
                        {
                            Label = "Negotiate quietly",
                            OutcomeText = "You offer a settlement and NDA.",
                            StatDeltas = new StatDeltas { RiskLevel = 2, BoardApproval = -1 },
                            CashDelta = -15000,
                            StockDelta = -0.7f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "labor_1",
                    Title = "Union Organizing Drive",
                    Category = "Labor",
                    Severity = 4,
                    Description = "Employees are organizing a union vote after workload increases.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Open negotiations",
                            OutcomeText = "You agree to a structured bargaining process.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 3, PublicReputation = 1 },
                            CashDelta = -8000,
                            StockDelta = -0.5f
                        },
                        new EventChoice
                        {
                            Label = "Fight the union",
                            OutcomeText = "Management pushes back hard.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -3, RiskLevel = 3 },
                            CashDelta = -2000,
                            StockDelta = 0.6f,
                            Flags = new List<string> { "union_bust" }
                        },
                        new EventChoice
                        {
                            Label = "Offer wage increases",
                            OutcomeText = "You raise wages to cool tensions.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 2, BoardApproval = 1 },
                            CashDelta = -12000,
                            StockDelta = -0.3f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "labor_2",
                    Title = "Layoff Recommendation",
                    Category = "Labor",
                    Severity = 3,
                    Description = "Finance recommends a 5% headcount reduction to hit targets.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Proceed with layoffs",
                            OutcomeText = "Costs drop but morale takes a hit.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -4, BoardApproval = 2, RiskLevel = 1 },
                            CashDelta = 15000,
                            StockDelta = 2.0f,
                            Flags = new List<string> { "layoff" }
                        },
                        new EventChoice
                        {
                            Label = "Use attrition instead",
                            OutcomeText = "You avoid sudden cuts at the expense of slower savings.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 1, BoardApproval = -1 },
                            CashDelta = 5000,
                            StockDelta = 0.5f
                        },
                        new EventChoice
                        {
                            Label = "Reject the idea",
                            OutcomeText = "You protect jobs but face budget pressure.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 2, BoardApproval = -2 },
                            CashDelta = -3000,
                            StockDelta = -1.0f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "labor_3",
                    Title = "Talent Retention Crisis",
                    Category = "Labor",
                    Severity = 2,
                    Description = "Top engineers are leaving for a competitor offering better perks.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Match the perks",
                            OutcomeText = "You match the perks to retain talent.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 3, BoardApproval = -1 },
                            CashDelta = -9000,
                            StockDelta = -0.2f
                        },
                        new EventChoice
                        {
                            Label = "Offer equity bonuses",
                            OutcomeText = "Equity bonuses keep key staff.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 2, BoardApproval = 1 },
                            CashDelta = -6000,
                            StockDelta = 0.4f
                        },
                        new EventChoice
                        {
                            Label = "Let them go",
                            OutcomeText = "You replace them with lower-cost hires.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -2, RiskLevel = 1 },
                            CashDelta = 3000,
                            StockDelta = -0.6f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "labor_4",
                    Title = "Workplace Safety Upgrade",
                    Category = "Labor",
                    Severity = 2,
                    Description = "Facilities request funding to improve safety equipment.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Approve upgrades",
                            OutcomeText = "Safety improvements reduce risk.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 2, RiskLevel = -2 },
                            CashDelta = -7000,
                            StockDelta = 0.3f
                        },
                        new EventChoice
                        {
                            Label = "Partial funding",
                            OutcomeText = "You fund the most critical fixes.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 1, RiskLevel = -1 },
                            CashDelta = -3000,
                            StockDelta = 0.1f
                        },
                        new EventChoice
                        {
                            Label = "Defer spending",
                            OutcomeText = "You delay the upgrades.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -2, RiskLevel = 2 },
                            CashDelta = 0,
                            StockDelta = -0.5f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "labor_5",
                    Title = "Town Hall Request",
                    Category = "Labor",
                    Severity = 1,
                    Description = "Employees ask for a quarterly town hall to address uncertainty.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Host the town hall",
                            OutcomeText = "You answer questions directly.",
                            StatDeltas = new StatDeltas { EmployeeMorale = 2, PublicReputation = 1 },
                            CashDelta = -1000,
                            StockDelta = 0.2f
                        },
                        new EventChoice
                        {
                            Label = "Send HR instead",
                            OutcomeText = "HR handles it, but employees want leadership.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -1 },
                            CashDelta = 0,
                            StockDelta = -0.1f
                        },
                        new EventChoice
                        {
                            Label = "Cancel the request",
                            OutcomeText = "You skip the event to focus on operations.",
                            StatDeltas = new StatDeltas { EmployeeMorale = -2 },
                            CashDelta = 0,
                            StockDelta = -0.3f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "board_1",
                    Title = "Activist Investor Letter",
                    Category = "Board",
                    Severity = 4,
                    Description = "An activist investor demands a breakup of the company to unlock value.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Engage and review",
                            OutcomeText = "You agree to review portfolio options.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, Power = -1 },
                            CashDelta = -4000,
                            StockDelta = 1.5f
                        },
                        new EventChoice
                        {
                            Label = "Reject publicly",
                            OutcomeText = "You push back and claim long-term vision.",
                            StatDeltas = new StatDeltas { Power = 1, BoardApproval = -2, RiskLevel = 1 },
                            CashDelta = 0,
                            StockDelta = -1.0f
                        },
                        new EventChoice
                        {
                            Label = "Offer board seats",
                            OutcomeText = "You offer seats to calm the activists.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, Power = -2 },
                            CashDelta = 0,
                            StockDelta = 0.8f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "board_2",
                    Title = "Strategy Pivot Debate",
                    Category = "Board",
                    Severity = 3,
                    Description = "Directors are split on shifting investment from legacy products to AI services.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Commit to AI pivot",
                            OutcomeText = "You commit to a bold pivot.",
                            StatDeltas = new StatDeltas { Power = 2, BoardApproval = 1, RiskLevel = 1 },
                            CashDelta = -12000,
                            StockDelta = 2.0f
                        },
                        new EventChoice
                        {
                            Label = "Keep a balanced portfolio",
                            OutcomeText = "You balance innovation with stability.",
                            StatDeltas = new StatDeltas { BoardApproval = 1 },
                            CashDelta = -6000,
                            StockDelta = 0.8f
                        },
                        new EventChoice
                        {
                            Label = "Stay with legacy",
                            OutcomeText = "You stick with what is working today.",
                            StatDeltas = new StatDeltas { BoardApproval = -1, Power = -1 },
                            CashDelta = 0,
                            StockDelta = -0.8f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "board_3",
                    Title = "Executive Succession Question",
                    Category = "Board",
                    Severity = 2,
                    Description = "A director asks about a formal succession plan.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Name an interim successor",
                            OutcomeText = "You name a trusted executive.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, Power = -1 },
                            CashDelta = -2000,
                            StockDelta = 0.3f
                        },
                        new EventChoice
                        {
                            Label = "Launch a review committee",
                            OutcomeText = "A committee will study succession options.",
                            StatDeltas = new StatDeltas { BoardApproval = 1 },
                            CashDelta = -1000,
                            StockDelta = 0.2f
                        },
                        new EventChoice
                        {
                            Label = "Reject the discussion",
                            OutcomeText = "You shut down the topic.",
                            StatDeltas = new StatDeltas { BoardApproval = -2, Power = 1 },
                            CashDelta = 0,
                            StockDelta = -0.6f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "board_4",
                    Title = "Dividend Pressure",
                    Category = "Board",
                    Severity = 3,
                    Description = "Board members want a higher dividend this quarter.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Increase dividend",
                            OutcomeText = "Shareholders cheer higher payouts.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, Power = 1 },
                            CashDelta = -20000,
                            StockDelta = 1.5f
                        },
                        new EventChoice
                        {
                            Label = "Offer a stock buyback",
                            OutcomeText = "You commit to a measured buyback.",
                            StatDeltas = new StatDeltas { BoardApproval = 1 },
                            CashDelta = -12000,
                            StockDelta = 1.0f
                        },
                        new EventChoice
                        {
                            Label = "Hold dividend steady",
                            OutcomeText = "You protect cash for investment.",
                            StatDeltas = new StatDeltas { BoardApproval = -2 },
                            CashDelta = 0,
                            StockDelta = -0.8f
                        }
                    }
                },
                new CorporateEvent
                {
                    Id = "board_5",
                    Title = "Board Ethics Review",
                    Category = "Board",
                    Severity = 4,
                    Description = "A director calls for an ethics review after a competitor scandal.",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Label = "Approve the review",
                            OutcomeText = "You approve a third-party review.",
                            StatDeltas = new StatDeltas { BoardApproval = 2, RiskLevel = -2, PublicReputation = 1 },
                            CashDelta = -6000,
                            StockDelta = 0.4f
                        },
                        new EventChoice
                        {
                            Label = "Run an internal review",
                            OutcomeText = "You keep it internal for speed.",
                            StatDeltas = new StatDeltas { BoardApproval = 1, RiskLevel = -1 },
                            CashDelta = -3000,
                            StockDelta = 0.2f
                        },
                        new EventChoice
                        {
                            Label = "Dismiss the idea",
                            OutcomeText = "You call it unnecessary.",
                            StatDeltas = new StatDeltas { BoardApproval = -3, RiskLevel = 2 },
                            CashDelta = 0,
                            StockDelta = -1.0f
                        }
                    }
                }
            };

            return events.ToDictionary(e => e.Id, e => e);
        }
    }
}
