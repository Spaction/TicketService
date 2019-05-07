var L = require('leaflet');

const freeMarker = {
    "fillColor": "#00ff00"
}

const heldMarker = {
    "fillColor": "#0000ff"
}

const reservedMarker = {
    "fillColor": "#ff0000"
}

const selected = {
    "color":"#ffff00",
    "fillColor":"#ffff00"
}


class TicketMap {
    constructor()
    {
        this.map = new L.Map("map", { crs: L.CRS.Simple, preferCanvas: true, maxBounds:[[-1,-1],[10,10]] ,maxZoom:8, minZoom:6});
        this.selected = {};
        this.map.setView(new L.LatLng(2,2),7);
    }

    removeData() {
        m.map.eachLayer(function (e) {
            m.map.removeLayer(e);
        });
    }

    refresh() {
        this.removeData();
        this.addData();
        this.selected = {};
    }


    addData() {
        $.get(dataUrl + ShowId)
            .done(function (d) {
                d.seats.forEach(x => {
                    drawSeat(x);

                })
            })
            .fail(function (d) {
                console.log(d);
            });
    }


}

function drawSeat(seat) {
    let circle = new L.CircleMarker(new L.LatLng(seat.loc.x, seat.loc.y), { fill: true });
    switch (seat.status) {
        case 0:
            circle.seatId = seat.seatId;
            L.setOptions(circle, freeMarker);
            addHold(circle,seat);
            break;
        case 1:
            L.setOptions(circle, heldMarker);
            circle.bindTooltip("This is currently held by: " + seat.reservedEmail + "<br>" + "With scrore: " + seat.rating);
            break;
        case 2:
            L.setOptions(circle, reservedMarker);
            circle.bindTooltip("This is currently reserved by: " + seat.reservedEmail);
            break;
    }
    circle.addTo(m.map);
}

function addHold(circle, seat) {
    circle.bindTooltip("This is Available and is seat: " + seat.seatId + "<br>" + "With scrore: " + seat.rating + "<br> Click me to try and hold");
    circle.on('click', function (e) {
        let c = new L.CircleMarker(e.target.getLatLng(), { radius: 15, fill: true })
        L.setOptions(c, selected);
        c.addTo(m.map);
        c.bringToBack();
        m.selected[e.target.seatId] = c;
        e.target.off('click')
        removeHold(e.target,seat);
    })
}

function removeHold(circle, seat) {
    circle.bindTooltip("This is Available and is seat: " + seat.seatId + "<br>" + "With scrore: " + seat.rating + "<br> Click me to unselect");
    circle.on('click', function (e) {
        let c = m.selected[e.target.seatId];
        m.map.removeLayer(c);
        delete m.selected[e.target.seatId];
        e.target.off('click');
        addHold(circle,seat);
    })
}

function SeatLoad(d) {
    //var l = new L.CircleMarker(d.)
    L.Util.setOptions()
}

var m;

function MapLoad() {
    m = new TicketMap();
    m.addData();
}

$("#HoldSelected").on('click',function()
{
    let intArr = [];
    for (let key of Object.keys(m.selected)) {
        intArr.push(parseInt(key));
    }
    

    let data = {
        ShowId: ShowId,
        customerEmail: $("#email").val(),
        seats: intArr
    }

    $.ajax({
        type: "POST",
        url: dataUrl + "Hold",
        data: JSON.stringify(data),
        contentType: "application/json"
    }).done(function (d) {
        m.refresh();
    })
    .fail(function (d) {
        console.log(d);
    })
})

$("#HoldX").on('click', function () {

    let data = {
        ShowId: ShowId,
        customerEmail: $("#email").val(),
        numSeats: $("#numSeats").val()
    }

    $.ajax({
        type: "POST",
        url: dataUrl + "Hold",
        data: JSON.stringify(data),
        contentType: "application/json"
    }).done(function (d) {
        m.refresh();
    })
    .fail(function (d) {
        console.log(d);
    })
})

$("#Reserve").on('click', function () {

    let intArr = [];
    for (let key of Object.keys(m.selected)) {
        intArr.push(parseInt(key));
    }


    let data = {
        ShowId: ShowId,
        customerEmail: $("#email").val()
        //seats: intArr
    }

    $.ajax({
        type: "POST",
        url: dataUrl + "ReserveSeats",
        data: JSON.stringify(data),
        contentType: "application/json"
    }).done(function (d) {
        m.refresh();
    })
        .fail(function (d) {
            console.log(d);
        })
})

$(document).ready(function () {
   MapLoad()
})