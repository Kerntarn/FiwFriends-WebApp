let map, marker, autocomplete;

function initMap() {
    const defaultLocation = { lat: 13.736717, lng: 100.523186 };

    map = new google.maps.Map(document.getElementById("map"), {
        center: defaultLocation,
        zoom: 13
    });

    marker = new google.maps.Marker({
        position: defaultLocation,
        map: map,
        draggable: true
    });

    google.maps.event.addListener(marker, 'dragend', function (event) {
        updateLocation(event.latLng);
    });

    const input = document.getElementById("locationInput");
    autocomplete = new google.maps.places.Autocomplete(input);
    autocomplete.bindTo("bounds", map);

    input.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
        }
    });

        autocomplete.addListener("place_changed", () => {
            const place = autocomplete.getPlace();
            if (!place.geometry) {
                alert("ไม่มีข้อมูลสถานที่ กรุณาเลือกใหม่");
                return;
            }
            updateLocation(place.geometry.location, place.name);
        });

        map.addListener("click", (event) => {
            updateLocation(event.latLng);
        });
}

function updateLocation(location, placeName = "") {
    marker.setPosition(location);
    map.panTo(location);

    if (!placeName) {
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: location }, (results, status) => {
            if (status === "OK" && results[0]) {
                document.getElementById("locationInput").value = results[0].formatted_address;
            } else {
                document.getElementById("locationInput").value = "Unknown Location";
            }
        });
    } else {
        document.getElementById("locationInput").value = placeName;
    }
}

function useCurrentLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const userLocation = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                };
                updateLocation(userLocation);
            },
            () => alert("ไม่สามารถดึงตำแหน่งปัจจุบันได้")
        );
    } else {
        alert("เบราว์เซอร์ของคุณไม่รองรับ Geolocation");
    }
}
