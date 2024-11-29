using System.Collections;
using Instalogik.Assembly.Ide.Client.Model;
using MudBlazor;

namespace Instalogik.Assembly.Ide.Client.Pages;

public partial class Home
{
    const string STEPS_ZONE = "Edytor";
    const string LIST_ZONE = "Instrukcje";

    private MudDropContainer<Instruction> _dropContainer;

    private List<Box> _boxes =
    [
        new("A"),
        new("B"),
        new("C"),
        new("D"),
    ];

    private List<DropZone> _zones =
    [
        new() { Name = STEPS_ZONE },
        new() { Name = LIST_ZONE }
    ];

    private List<Instruction> _items =
    [
        new PrintBoxInstruction(LIST_ZONE),
        new PrintTextInstruction(LIST_ZONE),
        new NewLineInstruction(LIST_ZONE),
        new LoadInstruction(LIST_ZONE),
        new SetInstruction(LIST_ZONE),
        new IncrementInstruction(LIST_ZONE),
        new DecrementInstruction(LIST_ZONE),
        new IfInstruction(LIST_ZONE),
        new LoadInstruction(LIST_ZONE)
    ];

    private void ItemUpdated(MudItemDropInfo<Instruction> dropItem)
    {
        if (dropItem.DropzoneIdentifier == _zones[0].Name && dropItem.Item.Zone == _zones[1].Name)
        {
            Instruction item = dropItem.Item.CopyTo(STEPS_ZONE) as Instruction;
            _items.Insert(dropItem.IndexInZone + 9, item);

            for (int i = 9; i < _items.Count; i++)
            {
                _items[i].Step = i - 8;
            }

            _dropContainer.Items = _items;
            _dropContainer.Refresh();
            _dropContainer.Refresh();
        }
        else
        {
            dropItem.Item.Zone = dropItem.DropzoneIdentifier;
        }
    }

    private List<string> GetJumpSteps(bool withNext = false)
    {
        List<string> result = [];
        if (withNext) result.Add("następnej");
        result.AddRange(_items.Where(x => x.Step > 0).Select(x => x.Step.ToString()).ToList());
        result.Add("końca");
        return result;
    }
}