using System.Collections.Generic;

namespace MacRegister.Model.Jems4Api
{
    public class StartWipResponse
    {
        public class DocumentResponse
        {
            public int RouteStepId { get; set; }
            public string RouteStepName { get; set; }
            public List<int> WipId { get; set; }
            public Document Document { get; set; }
        }

        public class Document
        {
            public List<Model> Model { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Model
        {
            public int Quantity { get; set; }
            public int DocumentId { get; set; }
            public string DocumentName { get; set; }
            public int PrinterId { get; set; }
            public int WipId { get; set; }
            public int ContainerId { get; set; }
            public int OrderId { get; set; }
            public int SerializedMaterialId { get; set; }
            public int WipAssembleHistoryId { get; set; }
            public List<DocumentField> DocumentFields { get; set; }
            public string DocumentDefinitionOverride { get; set; }
            public string LabelStockOverride { get; set; }
            public string RibbonStockOverride { get; set; }
            public string CarrierId { get; set; }
            public string CarrierContainerNumber { get; set; }
            public bool DocumentPrintEvent { get; set; }
        }

        public class DocumentField
        {
            public int DocumentFieldId { get; set; }
            public string FieldData { get; set; }
            public string FieldData2 { get; set; }
            public string FieldName { get; set; }
            public int FieldOrder { get; set; }
            public bool IsRepeating { get; set; }
            public bool IsRequired { get; set; }
            public int ListTypeId { get; set; }
            public int PageNumber { get; set; }
            public int TotalPages { get; set; }
            public string ParsedValue { get; set; }
            public bool IsOutputFromCustomProcedure { get; set; }
            public int WipId { get; set; }
            public List<string> ValidationRules { get; set; }
        }
    }
}
