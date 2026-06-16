export function CityMap({ lat, lon }) {
  const src = `https://www.openstreetmap.org/export/embed.html?bbox=${lon - 0.5},${lat - 0.5},${lon + 0.5},${lat + 0.5}&layer=mapnik&marker=${lat},${lon}`
  return (
    <div className="city-map">
      <iframe
        title="City location map"
        src={src}
        style={{ border: 0, width: '100%', height: '100%' }}
        loading="lazy"
        sandbox="allow-scripts allow-same-origin"
      />
    </div>
  )
}
