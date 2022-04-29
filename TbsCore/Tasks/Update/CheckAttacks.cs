﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TbsCore.Models.AccModels;
using TbsCore.Models.SendTroopsModels;
using TbsCore.Models.VillageModels;
using TbsCore.Parsers;
using TbsCore.Tasks.LowLevel;

namespace TbsCore.Tasks.Update
{
    public class CheckAttacks : BotTask
    {
        public override async Task<TaskRes> Execute(Account acc)
        {
            //await NavigationHelper.ToRallyPoint(acc, Vill, NavigationHelper.RallyPointTab.Overview)
            await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/build.php?gid=16&tt=1&filter=1&subfilters=1");

            var attacks = TroopsMovementParser.ParseTroopsOverview(acc, acc.Wb.Html);

            var pageCnt = TroopsMovementParser.GetPageCount(acc.Wb.Html);

            // Parsed all incoming attacks
            if (pageCnt == 1) return CheckCompleted(acc, attacks);

            // If there are multiple pages of attacks and attacks per page is less than 50,
            // increase number of attacks per page in preferences, then repeat CheckAttacks
            if (3 <= pageCnt && attacks.Count < 50)
            {
                acc.Tasks.Add(new EditPreferences()
                {
                    TroopsPerPage = 99, // Max
                    ExecuteAt = DateTime.MinValue.AddHours(1),
                    NextTask = new CheckAttacks() { Vill = this.Vill },
                });
                return TaskRes.Executed;
            }

            // Check all pages of the attacks
            int page = 1;
            do
            {
                await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/build.php?gid=16&tt=1&filter=1&subfilters=1&page={++page}");
                var pageAttacks = TroopsMovementParser.ParseTroopsOverview(acc, acc.Wb.Html);
                attacks.AddRange(pageAttacks);
                await Task.Delay(AccountHelper.Delay(acc));
            }
            while (!acc.Wb.Html // While the next page img isn't disabled
                .GetElementbyId("build")
                .Descendants("div")
                .First(x => x.HasClass("paginator"))
                .Descendants("img")
                .First()
                .HasClass("disabled")
                );

            return CheckCompleted(acc, attacks);
        }

        /// <summary>
        /// After getting all attacks, check for differences in attacks, alert user and
        /// configure this task for next check
        /// </summary>
        /// <param name="attacks">Incoming attacks</param>
        /// <returns>TaskRes.Executed</returns>
        private TaskRes CheckCompleted(Account acc, List<TroopsMovementRallyPoint> attacks)
        {
            // There are no attacks on the village
            if (attacks.Count == 0 || Vill.Deffing.AlertType == AlertTypeEnum.Disabled) return TaskRes.Executed;

            // In case it was null
            if (Vill.TroopMovements.IncomingAttacks == null) Vill.TroopMovements.IncomingAttacks = new List<TroopsMovementRallyPoint>();

            for (var i = 0; i < Vill.TroopMovements.IncomingAttacks.Count; i++)
            {
                var oldAttack = Vill.TroopMovements.IncomingAttacks[i];
                // Attack already happen
                if (DateTime.Compare(DateTime.Now, oldAttack.Arrival) > 0)
                {
                    Vill.TroopMovements.IncomingAttacks.RemoveAt(i);
                    continue;
                }

                // Remove all attacks that were discovered previously
                attacks.RemoveAll(x => x.Equals(oldAttack));
            }

            // Alert user if new attacks were found
            string alertStr = "";
            foreach (var newAttack in attacks)
            {
                // Check if hero is present in the attack
                if (Vill.Deffing.OnlyAlertOnHero && newAttack.Troops[10] == 0) continue;

                if (newAttack.MovementType == Classificator.MovementTypeRallyPoint.inRaid)
                {
                    if (Vill.Deffing.AlertType == AlertTypeEnum.AnyAttack)
                    {
                        alertStr += $"Raid from {newAttack.SourceCoordinates} at {newAttack.Arrival}\n";
                    }
                }
                else alertStr += $"Normal attack from {newAttack.SourceCoordinates} at {newAttack.Arrival} (server time)\n";
            }

            if (!String.IsNullOrEmpty(alertStr))
            {
                //send to discord webhook
                if (acc.Settings.DiscordWebhook)
                {
                    new Thread(() =>
                       DiscordHelper.SendMessage(acc, $"Village {Vill.Name} is under {attacks.Count} new attacks!\n{alertStr}")
                        ).Start();
                }
                else
                {
                    // Popup + sound
                    new Thread(() =>
                        IoHelperCore.AlertUser?.Invoke($"Village {Vill.Name} is under {attacks.Count} new attacks!\n{alertStr}")
                    ).Start();
                }
            }

            Vill.TroopMovements.IncomingAttacks.AddRange(attacks);
            // Next check for new attacks should be in:
            // - 1x speed = 30 min
            // - 3x speed = 10 min
            // - 5x speed = 6 min
            var nextCheckMin = 30 / acc.AccInfo.ServerSpeed;
            this.NextExecute = DateTime.Now.AddMinutes(Math.Max(nextCheckMin, 2)); // For TTWars

            return TaskRes.Executed;
        }
    }
}