using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiLab_9
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RoomTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            Document doc = commandData.Application.ActiveUIDocument.Document;
            
            var familySymbol = new FilteredElementCollector(doc)
               .OfClass(typeof(FamilySymbol))
               .OfCategory(BuiltInCategory.OST_RoomTags)
               .OfType<FamilySymbol>()
               .Where(x => x.FamilyName.Equals("Марка помещений"))
               .FirstOrDefault();
            if (familySymbol == null)
            {
                TaskDialog.Show("Ошибка", "Не найдено семейство \"Марка помещений\"");
                return Result.Succeeded;
            }


            List<Level> levels = Levels(doc);
            CreatRoom(doc, levels);

            return Result.Succeeded;


        }

        private static List<Level> Levels(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .OfType<Level>()
                .ToList();
        }

        private static void CreatRoom(Document doc, List<Level> levels)
        {
            

            Transaction tr = new Transaction(doc);
            tr.Start("Создание комнаты и марки ");
            int lev = 1;
            foreach (Level level in levels)
            {
                PlanCircuit planCircuit = null;
                PlanTopology planTopology = doc.get_PlanTopology(level);
                foreach (PlanCircuit circuit in planTopology.Circuits)
                {

                    if (null != circuit)
                    {

                        planCircuit = circuit;
                        Room room = doc.Create.NewRoom(null, planCircuit);
                        room.Name = $"{lev}_{room.Number}";


                    }
                }
                lev += 1;
            }

            tr.Commit();
        }
    }
}
