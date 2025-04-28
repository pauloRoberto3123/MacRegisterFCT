using System;
using System.Collections.Generic;

namespace MacRegister.Model.Jems4Api
{
    public class GetWipInformationBySerialNumberResponse
    {
        public int WipId { get; set; }
        public string SerialNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int AssemblyId { get; set; }
        public string AssemblyNumber { get; set; }
        public string AssemblyDescription { get; set; }
        public string AssemblyRevision { get; set; }
        public string AssemblyVersion { get; set; }
        public int PlannedOrderId { get; set; }
        public string PlannedOrderNumber { get; set; }
        public bool IsOnHold { get; set; }
        public bool IsScrapped { get; set; }
        public bool IsPacked { get; set; }
        public bool IsReferenceUnit { get; set; }
        public bool IsAssembled { get; set; }
        public string WipStatus { get; set; }
        public DateTime WipCreationDate { get; set; }
        public object ParentWip { get; set; }
        public List<InQueueRouteStep> InQueueRouteSteps { get; set; }
        public Panel Panel { get; set; }
    }

    public class InQueueRouteStep
    {
        public int InQueueRouteStepId { get; set; }
        public string InQueueRouteStepName { get; set; }
        public string InQueueRouteStepRouteName { get; set; }
    }

    public class Panel
    {
        public int PanelId { get; set; }
        public string PanelSerialNumber { get; set; }
        public string ConfiguredWipPerPanel { get; set; }
        public string ActualWipPerPanel { get; set; }
        public List<PanelWip> PanelWips { get; set; }
    }

    public class PanelWip
    {
        public int WipId { get; set; }
        public string SerialNumber { get; set; }
        public int PanelPosition { get; set; }
        public bool IsPanelBroken { get; set; }
    }

}

