//function PaintTareasInBolsaTareas() {
//    var lista = $('.padder #note-items');
//    lista.empty();
//    $.getJSON("/Admin/GetTareasInBolsaTareas", function (data) {
//        $.each(data, function (index, itemData) {
//            var stringHtml = '<li class="list-group-item hover active"><div class="rutas" id="' + itemData.IdTarea + '"><div class="view" id="note-1">' +
//                                '<button class="destroy close hover-action delTarea" onclick="DeleteTarea(' + itemData.IdTarea + ')">×</button>' +
//                                '<div class="note-name"> <strong>' + itemData.NombreTarea + '</strong> </div>' +
//                                '<div class="note-desc">' + itemData.Descripcion + '</div>' +
//                                '<span class="text-xs text-muted">' + itemData.Servicio.NombreServicio + '</span>' +
//                                '<div>&nbsp;</div>' +
//                            '</div></div></li>';
//            lista.append(stringHtml);
//        });
//    });
//}