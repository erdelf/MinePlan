using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MinePlan
{
    public class Designator_PlanMine : Designator
    {
        public bool didWeDesignateAnything = false;

        public override int DraggableDimensions => 2;

        public Designator_PlanMine()
        {
            this.defaultLabel = "Plan to Mine";
            this.icon = ContentFinder<Texture2D>.Get("MTP/PlanMine", true);
            this.defaultDesc = "Quickly change planning to mining designations";
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandardChanged;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Haul;
            DesignationCategoryDef named = DefDatabase<DesignationCategoryDef>.GetNamed("Orders", true);
            Type type = named.specialDesignatorClasses.Find((Type x) => x == GetType());
            if (type == null)
            {
                named.specialDesignatorClasses.Add(GetType());
                named.ResolveReferences();
                DesignationCategoryDef named2 = DefDatabase<DesignationCategoryDef>.GetNamed("OrdersPlanMine", true);
                List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
                allDefsListForReading.Remove(named2);
                DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Find.VisibleMap))
            {
                return AcceptanceReport.WasRejected;
            }
            if (Find.VisibleMap.designationManager.DesignationAt(c, DesignationDefOf.Plan) == null)
            {
                return AcceptanceReport.WasRejected;
            }
            if (c.Fogged(Find.VisibleMap))
            {
                return AcceptanceReport.WasAccepted;
            }
            Thing thing = c.GetFirstMineable(Find.VisibleMap);
            if (thing == null)
            {
                return "MessageMustDesignateMineable".Translate();
            }
            AcceptanceReport result = this.CanDesignateThing(thing);
            if (!result.Accepted)
            {
                return result;
            }
            return AcceptanceReport.WasAccepted;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (Find.VisibleMap.designationManager.DesignationAt(t.Position, DesignationDefOf.Plan) != null && t.def.mineable)
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasRejected;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            Designation des = Find.VisibleMap.designationManager.DesignationAt(loc, DesignationDefOf.Plan);
            if (des != null)
            {
                Find.VisibleMap.designationManager.RemoveDesignation(des);
                if (Find.VisibleMap.designationManager.DesignationAt(loc, DesignationDefOf.Mine) == null)
                    Find.VisibleMap.designationManager.AddDesignation(new Designation(loc, DesignationDefOf.Mine));
            }
        }

        public override void DesignateThing(Thing t) => DesignateSingleCell(t.Position);

        public override void SelectedUpdate() => GenUI.RenderMouseoverBracket();
    }
}
