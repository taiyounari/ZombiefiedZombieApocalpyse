using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Zombiefied
{
    // Token: 0x020000A4 RID: 164
    public class JobGiver_ZombieResponse : ThinkNode_JobGiver
    {
        // Token: 0x060003E6 RID: 998 RVA: 0x0002923C File Offset: 0x0002763C
        protected override Job TryGiveJob(Pawn pawn)
        {
            return this.TryGetAttackNearbyEnemyJob(pawn);
        }
        private static int checkInterval = 500;
        private static int tickAtLastCheck = -checkInterval;

         
        static List<Pawn> possiblePrey= new List<Pawn>();
        // Token: 0x060003E7 RID: 999 RVA: 0x000292AC File Offset: 0x000276AC
        private Job TryGetAttackNearbyEnemyJob(Pawn pawn)
        {
            if(tickAtLastCheck+checkInterval<=Find.TickManager.TicksGame)
            {
                possiblePrey.Clear();
                List<Thing>  allThingInMap = pawn.Map.listerThings.AllThings;
                foreach(Thing i in allThingInMap)
                {
                    Pawn otherPawn = i as Pawn;
                    if (otherPawn != null && otherPawn != pawn)
                    {
                       
                        possiblePrey.Add(otherPawn);
                        
                    }
                }
                tickAtLastCheck = Find.TickManager.TicksGame;

            }

            Pawn thing = BestPawnToHuntForPredator(pawn);
            if (thing != null)
            {
                return new Job(ZombiefiedMod.zombieHunt, thing)
                {
                    killIncappedTarget = true,
                    expiryInterval = (int)(Rand.RangeSeeded(1f, 2f, Find.TickManager.TicksAbs) * 700),
                    attackDoorIfTargetLost = true
                };
            }
            else
            {
               if (preyIsNear(pawn))                
                {
                    Thing building = BestNearbyBuilding(pawn);
                    if (building != null)
                    {
                        return new Job(ZombiefiedMod.zombieTrashBuilding, building)
                        {
                            maxNumMeleeAttacks = Rand.RangeSeeded(3, 7, Find.TickManager.TicksAbs + pawn.thingIDNumber),
                            expiryInterval = (int)(Rand.RangeSeeded(1f, 2f, Find.TickManager.TicksAbs) * 600)
                        };
                    } else
                    return null;
                } else
                return null;
            }
        }
        private Thing BestNearbyBuilding(Pawn destroyer, float range = 7f)
        {
            
            Thing thing = null;
            CellRect cellRect = CellRect.CenteredOn(destroyer.Position,(int)range);
            for (int i = 0; i < 35; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                if (randomCell.InBounds(destroyer.Map))
                {
                    // && TrashUtility.ShouldTrashBuilding(destroyer, edifice, false)&& GenSight.LineOfSight(destroyer.Position, randomCell, destroyer.Map, false, null, 0, 0))
                    Building edifice = randomCell.GetEdifice(destroyer.Map);
                    if (edifice != null)
                    {
                        if (Trashable(edifice))
                        {
                            thing = edifice;
                            break;
                        }
                    }
                }
            }
            return thing;
        }
        public static bool Trashable(Building b)
        {
            if(!b.def.useHitPoints ||
               (b.def.building != null && b.def.building.ai_neverTrashThis))
            {
                return false;
            }

            if(b.def.building.isTrap)
            {
                return false;
            }
            return true;
        }
        private bool preyIsNear(Pawn predator, float range = 7f)
        {
            bool isNear = false;
            /*
            foreach(Thing thing in allThingsRegion)
            {
                Pawn pawn = thing as Pawn;
                if(pawn!=predator&&pawn!=null)
                if(IsAcceptablePreyFor(predator,pawn,range)) {
                    isNear = true;
                    break;
                }
            }*/
            foreach(Pawn prey in possiblePrey)
            {
                if(IsAcceptablePreyFor(predator,prey,range))
                {
                    isNear = true;
                    break;
                }
            }
            return isNear;
        }
        public Pawn BestPawnToHuntForPredator(Pawn predator, float range = 7f)
        {
            //List<Pawn> allPawnsSpawned = predator.Map.mapPawns.AllPawnsSpawned;
           // List<Thing> allThingsRegion = predator.GetRegion().ListerThings.AllThings;

            Pawn pawnToReturn = null;
            float num = 0f;

            //for (int i = 0; i < allPawnsSpawned.Count; i++)
            /*
            foreach (Thing thing in allThingInMap)
                {
                Pawn pawn2 = thing as Pawn;
                if (pawn2 != null && predator != pawn2)
                {
                    if (IsAcceptablePreyFor(predator, pawn2, range))
                    {
                        if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            if (!pawn2.IsForbidden(predator))
                            {
                                float preyScoreFor = GetPreyScoreFor(predator, pawn2);
                                if (preyScoreFor > num || pawnToReturn == null)
                                {
                                    num = preyScoreFor;
                                    pawnToReturn = pawn2;
                                }
                            }
                        }
                    }
                }
            }*/
            foreach(Pawn prey in possiblePrey)
            {
                if (IsAcceptablePreyFor(predator, prey, range))
                {
                    if (predator.CanReach(prey, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        if (!prey.IsForbidden(predator))
                        {
                            float preyScoreFor = GetPreyScoreFor(predator, prey);
                            if (preyScoreFor > num || pawnToReturn == null)
                            {
                                num = preyScoreFor;
                                pawnToReturn = prey;
                            }
                        }
                    }
                }
            }
            return pawnToReturn;
        }

        public bool IsAcceptablePreyFor(Pawn predator, Pawn prey, float distance)
        {
            Pawn_Zombiefied preyZ = prey as Pawn_Zombiefied;
            if (preyZ != null)
            {
                return false;
            }         
            if(ZombiefiedMod.disableZombiesAttackingAnimals && !prey.RaceProps.Humanlike)
            {
                return false;
            }
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }
            float lengthHorizontal = -GetPreyScoreFor(predator, prey);
            if (lengthHorizontal > distance)
            {
                return false;
            }
            
            return true;
        }

        public float GetPreyScoreFor(Pawn predator, Pawn prey)
        {
            float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
            return -lengthHorizontal;
        }
    }
}
