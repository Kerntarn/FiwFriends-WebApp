let map, marker, autocomplete;

    function initMap() {
        const defaultLocation = { lat: 13.736717, lng: 100.523186 }; // พิกัดเริ่มต้นที่กรุงเทพฯ

        // สร้างแผนที่
        map = new google.maps.Map(document.getElementById("map"), {
            center: defaultLocation,
            zoom: 13
        });

        // สร้าง Marker บนแผนที่
        marker = new google.maps.Marker({
            position: defaultLocation,
            map: map,
            draggable: true // สามารถลาก Marker ได้
        });

        // เมื่อ Marker ถูกลาก ให้เปลี่ยนตำแหน่ง
        google.maps.event.addListener(marker, 'dragend', function (event) {
            updateLocation(event.latLng);
        });

        // เพิ่ม Autocomplete สำหรับช่องป้อนที่อยู่
        const input = document.getElementById("locationInput");
        autocomplete = new google.maps.places.Autocomplete(input);
        autocomplete.bindTo("bounds", map);

        // ป้องกัน Enter ส่งฟอร์ม
        input.addEventListener("keydown", (event) => {
            if (event.key === "Enter") {
                event.preventDefault(); // ป้องกันฟอร์ม submit
            }
        });

        // เมื่อเลือกสถานที่จาก Autocomplete
        autocomplete.addListener("place_changed", () => {
            const place = autocomplete.getPlace();
            if (!place.geometry) {
                alert("ไม่มีข้อมูลสถานที่ กรุณาเลือกใหม่");
                return;
            }
            updateLocation(place.geometry.location, place.name);
        });

        // เมื่อคลิกบนแผนที่ ให้ Marker ย้ายไปตำแหน่งนั้น
        map.addListener("click", (event) => {
            updateLocation(event.latLng);
        });
    }

    // ฟังก์ชันอัปเดตตำแหน่ง Marker และกรอกข้อมูลลงในช่อง Input
    function updateLocation(location, placeName = "") {
        marker.setPosition(location);
        map.panTo(location);

        // ใช้ Reverse Geocoding หาชื่อสถานที่ (ถ้าไม่ได้เลือกจาก Autocomplete)
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
